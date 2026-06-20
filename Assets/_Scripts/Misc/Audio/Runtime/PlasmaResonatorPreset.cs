using UnityEngine;

namespace _Scripts.Misc.Audio
{
    /// <summary>
    /// Plasma Resonator のパラメータ設定をアセットとして保存するための ScriptableObject。
    /// ゲームの方向性に合わせて様々な音響特性のプリセットを作成・再利用できます。
    /// </summary>
    [CreateAssetMenu(fileName = "PlasmaResonatorPreset", menuName = "Audio/Plasma Resonator Preset", order = 1)]
    public class PlasmaResonatorPreset : ScriptableObject
    {
        public PlasmaResonatorParameters parameters = PlasmaResonatorParameters.Default;

        /// <summary>
        /// パラメータを特定のプリセットタイプに設定する（ユーティリティ）。
        /// </summary>
        public void LoadPreset(PresetType type)
        {
            switch (type)
            {
                case PresetType.PlasmaCannon:
                    // Plasma Cannon: 強い resonance, 中程度 shift, 重い saturation
                    parameters.DelayTimeMs = 8f;
                    parameters.Feedback = 0.85f;
                    parameters.CombDamping = 0.1f;
                    parameters.ShiftHz = 280f;
                    parameters.Drive = 6.0f; // 重い saturation
                    parameters.LowpassBaseCutoff = 5000f;
                    parameters.Resonance = 4.5f; // 強い resonance
                    parameters.DynamicAmount = 0.4f;
                    parameters.Wet = 0.8f;
                    parameters.Dry = 0.4f;
                    break;

                case PresetType.AlienShield:
                    // Alien Shield: 高 shift, 長め delay, soft resonance
                    parameters.DelayTimeMs = 28f; // 長め delay
                    parameters.Feedback = 0.6f;
                    parameters.CombDamping = 0.4f;
                    parameters.ShiftHz = 750f; // 高 shift
                    parameters.Drive = 1.5f;
                    parameters.LowpassBaseCutoff = 12000f;
                    parameters.Resonance = 0.5f; // soft resonance (low Q)
                    parameters.DynamicAmount = 0.2f;
                    parameters.Wet = 0.7f;
                    parameters.Dry = 0.7f;
                    break;

                case PresetType.WarpEngine:
                    // Warp Engine: modulation（Shift）強め, dynamic LPF 強め
                    parameters.DelayTimeMs = 18f;
                    parameters.Feedback = 0.75f;
                    parameters.CombDamping = 0.2f;
                    parameters.ShiftHz = 480f; // modulation強め
                    parameters.Drive = 2.5f;
                    parameters.LowpassBaseCutoff = 9000f;
                    parameters.Resonance = 2.0f;
                    parameters.DynamicAmount = 0.85f; // dynamic LPF強め
                    parameters.Wet = 0.9f;
                    parameters.Dry = 0.3f;
                    break;
            }
        }
    }

    public enum PresetType
    {
        Custom,
        PlasmaCannon,
        AlienShield,
        WarpEngine
    }
}
