using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// Plasma Resonator を Unity の AudioSource に適用するための MonoBehaviour ラッパー。
    /// OnAudioFilterRead コールバックを通じて、アタッチされた AudioSource の音声をリアルタイム加工します。
    /// インスペクターからの調整、プリセットの読込、パラメータのランタイム・オートメーション（LFO等）に対応します。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlasmaResonatorController : MonoBehaviour
    {
        [Header("Preset")]
        [Tooltip("パラメータの初期設定に使用するプリセットアセット。")]
        [SerializeField] private PlasmaResonatorPreset preset;
        
        [Tooltip("標準プリセットを直接選択してロードできます。")]
        [SerializeField] private PresetType quickPreset = PresetType.Custom;

        [Header("Parameters")]
        [SerializeField] private PlasmaResonatorParameters parameters = PlasmaResonatorParameters.Default;

        [Header("Automation (オートメーション)")]
        [Tooltip("有効にすると、パラメータを時間経過で自動モジュレーション（揺らぎ）させます。")]
        [SerializeField] private bool enableAutomation = false;

        [Tooltip("オートメーション対象のパラメータ。")]
        [SerializeField] private AutomationTarget automationTarget = AutomationTarget.ShiftHz;

        [Tooltip("オートメーションLFOの速度 (Hz)。")]
        [SerializeField] [Range(0.1f, 20f)] private float lfoSpeed = 1f;

        [Tooltip("オートメーションLFOの深さ（適用率）。")]
        [SerializeField] [Range(0f, 1f)] private float lfoDepth = 0.5f;

        private PlasmaResonatorFilter filter;
        private float originalValueForAutomation;
        private float timeAccumulator;
        private int outputSampleRate;

        public PlasmaResonatorParameters Parameters
        {
            get => parameters;
            set
            {
                parameters = value;
                if (filter != null) filter.Parameters = parameters;
            }
        }

        private void Awake()
        {
            filter = new PlasmaResonatorFilter();
            ApplyPreset();
            filter.Parameters = parameters;
            outputSampleRate = AudioSettings.outputSampleRate;
        }

        private void OnEnable()
        {
            // パラメータを同期
            if (filter != null) filter.Parameters = parameters;
        }

        private void Start()
        {
            // ロード時の初期自動化パラメータ値を保持
            StoreOriginalValue();
        }

        private void Update()
        {
            // ランタイムでのパラメータ・オートメーション
            if (enableAutomation)
            {
                timeAccumulator += Time.deltaTime;
                float lfo = Mathf.Sin(timeAccumulator * 2f * Mathf.PI * lfoSpeed); // -1.0 〜 1.0

                switch (automationTarget)
                {
                    case AutomationTarget.ShiftHz:
                        // ShiftHz を LFO で揺らす
                        float maxShiftMod = 500f * lfoDepth;
                        parameters.ShiftHz = Mathf.Clamp(originalValueForAutomation + lfo * maxShiftMod, 0f, 1000f);
                        break;

                    case AutomationTarget.LowpassBaseCutoff:
                        // カットオフを LFO で揺らす (オクターブ揺らぎのシミュレート)
                        float maxCutoffMod = originalValueForAutomation * 0.5f * lfoDepth;
                        parameters.LowpassBaseCutoff = Mathf.Clamp(originalValueForAutomation + lfo * maxCutoffMod, 50f, 20000f);
                        break;

                    case AutomationTarget.Feedback:
                        // コームフィードバックを揺らす
                        float maxFbMod = 0.2f * lfoDepth;
                        parameters.Feedback = Mathf.Clamp(originalValueForAutomation + lfo * maxFbMod, 0f, 0.95f);
                        break;
                }
            }

            // リアルタイムインスペクターの変更を反映
            if (filter != null)
            {
                filter.Parameters = parameters;
            }
        }

        private void OnValidate()
        {
            // インスペクターの変更があった場合のクイックプリセット処理
            if (quickPreset != PresetType.Custom)
            {
                ApplyQuickPreset();
            }
            else if (preset != null)
            {
                parameters = preset.parameters;
            }

            if (filter != null)
            {
                filter.Parameters = parameters;
            }
            StoreOriginalValue();
        }

        private void OnDisable()
        {
            // フィルタの解放（NativeArrayの破棄など）
            filter?.Dispose();
        }

        private void OnDestroy()
        {
            filter?.Dispose();
        }

        /// <summary>
        /// アタッチされたプリセットデータを反映する。
        /// </summary>
        public void ApplyPreset()
        {
            if (preset != null)
            {
                parameters = preset.parameters;
                quickPreset = PresetType.Custom;
            }
            else if (quickPreset != PresetType.Custom)
            {
                ApplyQuickPreset();
            }
            
            if (filter != null) filter.Parameters = parameters;
            StoreOriginalValue();
        }

        private void ApplyQuickPreset()
        {
            // アセットを経由しない即席ロード
            var tempPreset = ScriptableObject.CreateInstance<PlasmaResonatorPreset>();
            tempPreset.LoadPreset(quickPreset);
            parameters = tempPreset.parameters;
            DestroyImmediate(tempPreset);
            StoreOriginalValue();
        }

        private void StoreOriginalValue()
        {
            switch (automationTarget)
            {
                case AutomationTarget.ShiftHz:
                    originalValueForAutomation = parameters.ShiftHz;
                    break;
                case AutomationTarget.LowpassBaseCutoff:
                    originalValueForAutomation = parameters.LowpassBaseCutoff;
                    break;
                case AutomationTarget.Feedback:
                    originalValueForAutomation = parameters.Feedback;
                    break;
            }
        }

        /// <summary>
        /// Unity のオーディオパイプラインの途中で割り込み処理を行うコールバック。
        /// </summary>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            filter?.Process(data, channels, outputSampleRate);
        }
    }

    public enum AutomationTarget
    {
        ShiftHz,
        LowpassBaseCutoff,
        Feedback
    }
}
