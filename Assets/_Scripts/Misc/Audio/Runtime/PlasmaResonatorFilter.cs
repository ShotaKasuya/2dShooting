using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// Plasma Resonator のパラメータセット。
    /// インスペクターやプリセットアセット、オートメーションで共有されます。
    /// </summary>
    [Serializable]
    public struct PlasmaResonatorParameters
    {
        [Header("Comb Filter (共鳴器)")] [Tooltip("ディレイ時間 (ms)。金属感や空間の広さをコントロールします。")] [Range(5f, 30f)]
        public float DelayTimeMs;

        [Tooltip("フィードバック量 (0.0〜0.95)。大きいほど共振が長く続き、金属的な響きになります。")] [Range(0f, 0.95f)]
        public float Feedback;

        [Tooltip("コーム高域減衰率。高音域 of 共振を抑え、マイルドにします。")] [Range(0f, 0.99f)]
        public float CombDamping;

        [Header("Frequency Shifter (周波数変調器)")] [Tooltip("シフト周波数 (Hz)。高エネルギー物体の異星感、SF的な不協和を作ります。")] [Range(0f, 1000f)]
        public float ShiftHz;

        [Header("Soft Saturation (歪み)")] [Tooltip("ドライブ量。音圧を上げ、デジタル臭さを除いてアナログ的な飽和感を作ります。")] [Range(1f, 10f)]
        public float Drive;

        [Header("Dynamic Lowpass (動的低域通過フィルタ)")] [Tooltip("基準カットオフ周波数 (Hz)。")] [Range(20f, 20000f)]
        public float LowpassBaseCutoff;

        [Tooltip("基準レゾナンス (Q値)。大きいほどカットオフ付近が発振し、尖った音になります。")] [Range(0.1f, 10.0f)]
        public float Resonance;

        [Tooltip("音量連動強度。入力音量が大きいほど、カットオフが低下しレゾナンスが増加します。")] [Range(0f, 1f)]
        public float DynamicAmount;

        [Header("Output Mix (出力比率)")] [Tooltip("ウェット音（エフェクト音）の音量レベル。")] [Range(0f, 1f)]
        public float Wet;

        [Tooltip("ドライ音（原音）の音量レベル。")] [Range(0f, 1f)]
        public float Dry;

        /// <summary>
        /// デフォルトパラメータを作成する。
        /// </summary>
        public static PlasmaResonatorParameters Default => new PlasmaResonatorParameters
        {
            DelayTimeMs = 15f,
            Feedback = 0.7f,
            CombDamping = 0.2f,
            ShiftHz = 150f,
            Drive = 2.0f,
            LowpassBaseCutoff = 8000f,
            Resonance = 1.0f,
            DynamicAmount = 0.5f,
            Wet = 0.7f,
            Dry = 0.5f
        };
    }

    /// <summary>
    /// Burstコンパイルに対応した、アンマネージド領域で動作するPlasma ResonatorのDSPコア状態。
    /// </summary>
    public struct PlasmaResonatorState
    {
        public CombFilter Comb;
        public FrequencyShifter Shifter;
        public SoftSaturation Saturator;
        public DynamicLowpass Lowpass;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        /// <summary>
        /// DSPの全モジュールを初期化する。
        /// </summary>
        public void Init(float sampleRate)
        {
            if (isInitialized) Dispose();

            Comb.Init(sampleRate);
            Shifter.Init();
            // Saturatorはステートレスのため初期化不要
            Lowpass.Init(sampleRate);

            isInitialized = true;
        }

        /// <summary>
        /// アンマネージドバッファを破棄する。
        /// </summary>
        public void Dispose()
        {
            if (!isInitialized) return;
            Comb.Dispose();
            isInitialized = false;
        }

        /// <summary>
        /// ステレオまたはモノラルデータを一括してインプレース処理する。
        /// </summary>
        public void Process(Span<float> data, int channels, int sampleRate, PlasmaResonatorParameters @params)
        {
            if (!isInitialized) return;

            int length = data.Length;
            for (int i = 0; i < length; i += channels)
            {
                float left = data[i];
                float right = (channels > 1) ? data[i + 1] : left;

                float dryLeft = left;
                float dryRight = right;

                // 1. Comb Filter (共振・空間的広がり)
                Comb.ProcessStereo(ref left, ref right, @params.DelayTimeMs, @params.Feedback, @params.CombDamping,
                    sampleRate);

                // 2. Frequency Shifter (不協和音・異星感)
                Shifter.ProcessStereo(ref left, ref right, @params.ShiftHz, sampleRate);

                // 3. Soft Saturation (音圧・飽和歪み)
                left = Saturator.Process(left, @params.Drive);
                right = Saturator.Process(right, @params.Drive);

                // 4. Dynamic Lowpass (音量連動フィルタ・熱暴走感)
                Lowpass.ProcessStereo(ref left, ref right, @params.LowpassBaseCutoff, @params.Resonance,
                    @params.DynamicAmount, sampleRate);

                // 5. Wet/Dry ミックス
                float outL = dryLeft * @params.Dry + left * @params.Wet;
                float outR = dryRight * @params.Dry + right * @params.Wet;

                // バッファに書き戻し
                data[i] = outL;
                if (channels > 1)
                {
                    data[i + 1] = outR;
                }
            }
        }
    }

    /// <summary>
    /// Burstコンパイル可能な処理ジョブ。
    /// 音声スレッドから同期実行することで、メインスレッドへのオーバーヘッドを最小化します。
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public ref struct PlasmaResonatorBurstJob
    {
        public Span<float> AudioData;
        public int Channels;
        public int SampleRate;
        public PlasmaResonatorParameters Parameters;

        // ジョブ内で状態を更新できるように参照型を避けるため、ポインタまたは直接操作可能な構造体として受け取る。
        // State内のNativeArrayが共有され、内部で書き換えが行われます。
        public PlasmaResonatorState State;

        public void Execute()
        {
            State.Process(AudioData, Channels, SampleRate, Parameters);
        }
    }

    /// <summary>
    /// IAudioFilter を実装した、マネージド環境用の Plasma Resonator フィルタ。
    /// </summary>
    public class PlasmaResonatorFilter : IAudioFilter, IDisposable
    {
        private PlasmaResonatorState state;
        private PlasmaResonatorParameters parameters;
        private float currentSampleRate;

        public PlasmaResonatorParameters Parameters
        {
            get => parameters;
            set => parameters = value;
        }

        public PlasmaResonatorFilter()
        {
            parameters = PlasmaResonatorParameters.Default;
        }

        /// <summary>
        /// 音声信号処理を実行する（IAudioFilter実装）。
        /// 内部でBurstジョブを起動し、GC Alloc 0 かつ極めて高速なリアルタイム信号処理を行います。
        /// </summary>
        public void Process(float[] buffer, int channels, int sampleRate)
        {
            if (buffer == null || buffer.Length == 0) return;

            // サンプリングレートが変更された場合は再初期化
            if (!state.IsInitialized || (int)currentSampleRate != sampleRate)
            {
                currentSampleRate = sampleRate;
                state.Init(sampleRate);
            }


            // ジョブの構成
            var job = new PlasmaResonatorBurstJob
            {
                AudioData = buffer,
                Channels = channels,
                SampleRate = sampleRate,
                Parameters = parameters,
                State = state
            };
            job.Execute();
        }

        public void Dispose()
        {
            state.Dispose();
        }
    }
}