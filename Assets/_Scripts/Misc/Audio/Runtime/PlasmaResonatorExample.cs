using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// PlasmaResonatorController の使用例を示すデモコンポーネント。
    /// 効果音（レーザー、シールド、ワープ等）のトリガー時における、
    /// パラメータ動的変化の制御方法を示します。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PlasmaResonatorController))]
    public class PlasmaResonatorExample : MonoBehaviour
    {
        private AudioSource audioSource;
        private PlasmaResonatorController resonatorController;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip laserClip;
        [SerializeField] private AudioClip shieldClip;
        [SerializeField] private AudioClip warpClip;

        [Header("Scriptable Presets")]
        [SerializeField] private PlasmaResonatorPreset plasmaCannonPreset;
        [SerializeField] private PlasmaResonatorPreset alienShieldPreset;
        [SerializeField] private PlasmaResonatorPreset warpEnginePreset;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            resonatorController = GetComponent<PlasmaResonatorController>();
        }

        private void Update()
        {
            // キーボード入力によるエフェクト音の試聴例 (デバッグ・動作確認用)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayPlasmaCannonLaser();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlayAlienShieldDeflect();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PlayWarpEngineStart();
            }
        }

        /// <summary>
        /// プラズマレーザーの発射音。
        /// プリセットパラメータを読み込み、発射直後にディレイ時間を自動で縮小させることで
        /// 「チャージされたプラズマが徐々に集束して発射される」動的ピッチ変化をコームフィルタで表現します。
        /// </summary>
        public void PlayPlasmaCannonLaser()
        {
            if (laserClip == null)
            {
                Debug.LogWarning("Laser Clip is not assigned!");
                return;
            }

            // 1. プラズマキャノンの音響プリセットを反映
            if (plasmaCannonPreset != null)
            {
                resonatorController.Parameters = plasmaCannonPreset.parameters;
            }
            else
            {
                // プリセットファイルが無い場合はコードで直接設定
                var parameters = PlasmaResonatorParameters.Default;
                parameters.DelayTimeMs = 8f;
                parameters.Feedback = 0.85f;
                parameters.CombDamping = 0.1f;
                parameters.ShiftHz = 280f;
                parameters.Drive = 6.0f;
                parameters.LowpassBaseCutoff = 5000f;
                parameters.Resonance = 4.5f;
                parameters.DynamicAmount = 0.4f;
                parameters.Wet = 0.8f;
                parameters.Dry = 0.4f;
                resonatorController.Parameters = parameters;
            }

            // 2. 音声を再生
            audioSource.clip = laserClip;
            audioSource.Play();

            Debug.Log("Played Plasma Cannon Laser (Press '1' to re-trigger)");
        }

        /// <summary>
        /// エイリアンシールドの防御音。
        /// 周波数シフタを極端に高周波にシフトさせることで、宇宙的で現実離れしたバリアー感を表現します。
        /// </summary>
        public void PlayAlienShieldDeflect()
        {
            if (shieldClip == null)
            {
                Debug.LogWarning("Shield Clip is not assigned!");
                return;
            }

            // 1. エイリアンシールドの音響プリセットを反映
            if (alienShieldPreset != null)
            {
                resonatorController.Parameters = alienShieldPreset.parameters;
            }
            else
            {
                // プリセットファイルが無い場合はコードで直接設定
                var parameters = PlasmaResonatorParameters.Default;
                parameters.DelayTimeMs = 28f;
                parameters.Feedback = 0.6f;
                parameters.CombDamping = 0.4f;
                parameters.ShiftHz = 750f;
                parameters.Drive = 1.5f;
                parameters.LowpassBaseCutoff = 12000f;
                parameters.Resonance = 0.5f;
                parameters.DynamicAmount = 0.2f;
                parameters.Wet = 0.7f;
                parameters.Dry = 0.7f;
                resonatorController.Parameters = parameters;
            }

            // 2. 音声を再生
            audioSource.clip = shieldClip;
            audioSource.Play();

            Debug.Log("Played Alien Shield Deflect (Press '2' to re-trigger)");
        }

        /// <summary>
        /// ワープエンジンの起動音。
        /// </summary>
        public void PlayWarpEngineStart()
        {
            if (warpClip == null)
            {
                Debug.LogWarning("Warp Clip is not assigned!");
                return;
            }

            // 1. ワープエンジンの音響プリセットを反映
            if (warpEnginePreset != null)
            {
                resonatorController.Parameters = warpEnginePreset.parameters;
            }
            else
            {
                // プリセットファイルが無い場合はコードで直接設定
                var parameters = PlasmaResonatorParameters.Default;
                parameters.DelayTimeMs = 18f;
                parameters.Feedback = 0.75f;
                parameters.CombDamping = 0.2f;
                parameters.ShiftHz = 480f;
                parameters.Drive = 2.5f;
                parameters.LowpassBaseCutoff = 9000f;
                parameters.Resonance = 2.0f;
                parameters.DynamicAmount = 0.85f;
                parameters.Wet = 0.9f;
                parameters.Dry = 0.3f;
                resonatorController.Parameters = parameters;
            }

            // 2. 音声を再生
            audioSource.clip = warpClip;
            audioSource.Play();

            Debug.Log("Played Warp Engine Start (Press '3' to re-trigger)");
        }
    }
}
