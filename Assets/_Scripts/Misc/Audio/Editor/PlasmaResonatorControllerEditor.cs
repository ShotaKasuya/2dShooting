using UnityEditor;
using UnityEngine;
using _Scripts.Misc.Audio;

namespace _Scripts.Misc.Audio.Editor
{
    /// <summary>
    /// PlasmaResonatorController のカスタムインスペクターUI。
    /// パラメータをDSPステージごとにグループ化し、プリセット変更やオートメーションの設定を整理します。
    /// </summary>
    [CustomEditor(typeof(PlasmaResonatorController))]
    public class PlasmaResonatorControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PlasmaResonatorController controller = (PlasmaResonatorController)target;

            // プリセットの描画
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preset Configuration", EditorStyles.boldLabel);
            
            SerializedProperty presetProp = serializedObject.FindProperty("preset");
            SerializedProperty quickPresetProp = serializedObject.FindProperty("quickPreset");
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(presetProp);
            EditorGUILayout.PropertyField(quickPresetProp);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                controller.ApplyPreset();
                serializedObject.Update();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("DSP Pipeline Parameters", EditorStyles.boldLabel);
            
            SerializedProperty parametersProp = serializedObject.FindProperty("parameters");
            
            // パラメータをグループごとにボックス表示して見やすくする
            if (parametersProp != null)
            {
                EditorGUILayout.BeginVertical("box");
                
                // 各ステージのプロパティを取得
                SerializedProperty delayProp = parametersProp.FindPropertyRelative("DelayTimeMs");
                SerializedProperty feedbackProp = parametersProp.FindPropertyRelative("Feedback");
                SerializedProperty dampingProp = parametersProp.FindPropertyRelative("CombDamping");
                
                SerializedProperty shiftProp = parametersProp.FindPropertyRelative("ShiftHz");
                
                SerializedProperty driveProp = parametersProp.FindPropertyRelative("Drive");
                
                SerializedProperty cutoffProp = parametersProp.FindPropertyRelative("LowpassBaseCutoff");
                SerializedProperty resonanceProp = parametersProp.FindPropertyRelative("Resonance");
                SerializedProperty dynamicProp = parametersProp.FindPropertyRelative("DynamicAmount");
                
                SerializedProperty wetProp = parametersProp.FindPropertyRelative("Wet");
                SerializedProperty dryProp = parametersProp.FindPropertyRelative("Dry");

                // Stage 1. Comb Filter
                EditorGUILayout.LabelField("Stage 1. Comb Filter (Resonator)", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(delayProp);
                EditorGUILayout.PropertyField(feedbackProp);
                EditorGUILayout.PropertyField(dampingProp);
                EditorGUILayout.Space();

                // Stage 2. Frequency Shifter
                EditorGUILayout.LabelField("Stage 2. Frequency Shifter (SSB Modulator)", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(shiftProp);
                EditorGUILayout.Space();

                // Stage 3. Soft Saturation
                EditorGUILayout.LabelField("Stage 3. Soft Saturation (tanh clip approximation)", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(driveProp);
                EditorGUILayout.Space();

                // Stage 4. Dynamic Lowpass
                EditorGUILayout.LabelField("Stage 4. Dynamic Lowpass (Envelope Follower + SVF)", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(cutoffProp);
                EditorGUILayout.PropertyField(resonanceProp);
                EditorGUILayout.PropertyField(dynamicProp);
                EditorGUILayout.Space();

                // Output Mix Stage
                EditorGUILayout.LabelField("Stage 5. Output Mix", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(wetProp);
                EditorGUILayout.PropertyField(dryProp);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("LFO Parameter Automation", EditorStyles.boldLabel);
            
            SerializedProperty enableAutoProp = serializedObject.FindProperty("enableAutomation");
            SerializedProperty autoTargetProp = serializedObject.FindProperty("automationTarget");
            SerializedProperty autoSpeedProp = serializedObject.FindProperty("lfoSpeed");
            SerializedProperty autoDepthProp = serializedObject.FindProperty("lfoDepth");

            EditorGUILayout.PropertyField(enableAutoProp);
            if (enableAutoProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(autoTargetProp);
                EditorGUILayout.PropertyField(autoSpeedProp);
                EditorGUILayout.PropertyField(autoDepthProp);
                EditorGUI.indentLevel--;
            }

            // クイックプリセットロード用の即時実行ボタン
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Trigger Presets Instantly", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Plasma Cannon"))
            {
                quickPresetProp.enumValueIndex = (int)PresetType.PlasmaCannon;
                serializedObject.ApplyModifiedProperties();
                controller.ApplyPreset();
            }
            if (GUILayout.Button("Alien Shield"))
            {
                quickPresetProp.enumValueIndex = (int)PresetType.AlienShield;
                serializedObject.ApplyModifiedProperties();
                controller.ApplyPreset();
            }
            if (GUILayout.Button("Warp Engine"))
            {
                quickPresetProp.enumValueIndex = (int)PresetType.WarpEngine;
                serializedObject.ApplyModifiedProperties();
                controller.ApplyPreset();
            }
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
