using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// エンベロープフォロワー (Envelope Follower)
    /// 入力オーディオ信号の瞬間的な音量包絡線（エンベロープ）を追跡します。
    /// アタック時間とリリース時間を考慮した1次ラグフィルタを使用します。
    /// </summary>
    public struct EnvelopeFollower
    {
        private float envelope;
        private float attackCoef;
        private float releaseCoef;

        /// <summary>
        /// アタック時間とリリース時間に基づいて係数を初期化する。
        /// </summary>
        public void Init(float sampleRate, float attackTimeMs = 10f, float releaseTimeMs = 100f)
        {
            envelope = 0f;
            
            // 係数の計算: coef = exp(-1 / (fs * time_seconds))
            attackCoef = Mathf.Exp(-1.0f / (sampleRate * attackTimeMs * 0.001f));
            releaseCoef = Mathf.Exp(-1.0f / (sampleRate * releaseTimeMs * 0.001f));
        }

        /// <summary>
        /// サンプルを入力して現在の音量レベル（エンベロープ値）を更新し取得する。
        /// </summary>
        public float Process(float input)
        {
            float envIn = Mathf.Abs(input);
            
            // 入力レベルが現在のエンベロープ値より大きければアタック、小さければリリース
            if (envIn > envelope)
            {
                envelope = envIn + attackCoef * (envelope - envIn);
            }
            else
            {
                envelope = envIn + releaseCoef * (envelope - envIn);
            }

            return envelope;
        }
    }

    /// <summary>
    /// 動的低域通過フィルタ (Dynamic Lowpass Filter)
    /// Chamberlin型ステート変数フィルタ (SVF: State Variable Filter) をベースに、
    /// エンベロープフォロワーからの入力音量変化量に応じて、カットオフ周波数と共鳴（レゾナンス）量を
    /// 動的に変化（モジュレーション）させます。
    /// 大音量時にカットオフが低下し、レゾナンスが強化されることで、「熱暴走」「エネルギー過負荷」感を表現します。
    /// </summary>
    public struct DynamicLowpass
    {
        private float lowL, bandL;
        private float lowR, bandR;
        private EnvelopeFollower envelopeFollower;

        /// <summary>
        /// フィルタ状態およびエンベロープフォロワーを初期化する。
        /// </summary>
        public void Init(float sampleRate)
        {
            lowL = bandL = 0f;
            lowR = bandR = 0f;
            
            // 標準的なアタック10ms、リリース120msでエンベロープフォロワーを初期化
            envelopeFollower.Init(sampleRate, 10f, 120f);
        }

        /// <summary>
        /// ステレオサンプルのカットオフと共鳴を動的にモジュレーションして低域通過フィルタリングを適用する。
        /// </summary>
        public void ProcessStereo(ref float left, ref float right, float baseCutoff, float resonance, float dynamicAmount, float sampleRate)
        {
            // 左右サンプルの平均からエンベロープを追跡
            float inputAmp = (Mathf.Abs(left) + Mathf.Abs(right)) * 0.5f;
            float env = envelopeFollower.Process(inputAmp);

            // 動的変調の適用 (大音量時: cutoff低下, resonance増加)
            // カットオフ周波数の減少量: 最大で baseCutoff * dynamicAmount
            float targetCutoff = baseCutoff - (dynamicAmount * env * baseCutoff);
            
            // 安全な周波数範囲に制限 (50Hz〜20000Hz)
            targetCutoff = Mathf.Clamp(targetCutoff, 50f, 20000f);

            // 共鳴量 (Q値) の増加量: 最大で + (dynamicAmount * 6.0)
            float targetQ = resonance + (dynamicAmount * env * 6.0f);
            
            // 安全なQ値範囲に制限 (0.1〜10.0)
            targetQ = Mathf.Clamp(targetQ, 0.1f, 10.0f);

            // Chamberlin SVF 係数計算
            // f = 2 * sin(pi * fc / fs)
            float f = 2f * Mathf.Sin(Mathf.PI * targetCutoff / sampleRate);
            
            // 安定化のためのリミット (f >= 2 で発振限界に達し不安定化するため1.9以下に制限)
            f = Mathf.Clamp(f, 0f, 1.9f);
            float q1 = 1.0f / targetQ;

            // 左チャンネルのフィルタ処理
            float highL = left - lowL - q1 * bandL;
            bandL = bandL + f * highL;
            lowL = lowL + f * bandL;

            // 右チャンネルのフィルタ処理
            float highR = right - lowR - q1 * bandR;
            bandR = bandR + f * highR;
            lowR = lowR + f * bandR;

            // LPF出力を適用
            left = lowL;
            right = lowR;
        }
    }
}
