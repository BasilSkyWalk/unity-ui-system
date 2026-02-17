using UnityEditor;
using UnityEngine;

namespace GOC.UISystem.Editor
{
    [CustomEditor(typeof(UISystemConfig))]
    public class UISystemConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _defaultScreenTransition;
        private SerializedProperty _defaultPopupTransition;
        private SerializedProperty _hudScreenKey;
        private SerializedProperty _fallbackScreenKey;
        private SerializedProperty _screenConfigs;
        private SerializedProperty _transitionOverrides;

        private bool _showScreenConfigs = true;
        private bool _showTransitionOverrides = true;

        private void OnEnable()
        {
            _defaultScreenTransition = serializedObject.FindProperty("DefaultScreenTransition");
            _defaultPopupTransition = serializedObject.FindProperty("DefaultPopupTransition");
            _hudScreenKey = serializedObject.FindProperty("HudScreenKey");
            _fallbackScreenKey = serializedObject.FindProperty("FallbackScreenKey");
            _screenConfigs = serializedObject.FindProperty("ScreenConfigs");
            _transitionOverrides = serializedObject.FindProperty("TransitionOverrides");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("UI System Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_defaultScreenTransition);
            EditorGUILayout.PropertyField(_defaultPopupTransition);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("HUD & Navigation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hudScreenKey);
            EditorGUILayout.PropertyField(_fallbackScreenKey);

            EditorGUILayout.Space(8);
            _showScreenConfigs = EditorGUILayout.Foldout(_showScreenConfigs, "Screen Configurations", true);
            if (_showScreenConfigs)
            {
                EditorGUI.indentLevel++;
                DrawScreenConfigs();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            _showTransitionOverrides = EditorGUILayout.Foldout(_showTransitionOverrides, "Transition Overrides", true);
            if (_showTransitionOverrides)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_transitionOverrides, true);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScreenConfigs()
        {
            for (int i = 0; i < _screenConfigs.arraySize; i++)
            {
                var element = _screenConfigs.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative("ScreenKey");
                var showHudProp = element.FindPropertyRelative("ShowHud");
                var usesPlayerProp = element.FindPropertyRelative("UsesPlayerInput");
                var excludeProp = element.FindPropertyRelative("ExcludeFromHistory");

                string label = string.IsNullOrEmpty(keyProp.stringValue) ? $"Screen {i}" : keyProp.stringValue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _screenConfigs.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(keyProp);
                EditorGUILayout.PropertyField(showHudProp);
                EditorGUILayout.PropertyField(usesPlayerProp);
                EditorGUILayout.PropertyField(excludeProp);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("Add Screen Config"))
            {
                _screenConfigs.InsertArrayElementAtIndex(_screenConfigs.arraySize);
            }
        }
    }
}
