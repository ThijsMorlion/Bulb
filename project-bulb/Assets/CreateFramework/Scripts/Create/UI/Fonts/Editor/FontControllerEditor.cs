using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Create.UI.Fonts
{
    [CustomEditor(typeof(FontController))]
    public class FontControllerEditor : Editor
    {
        private List<bool> _foldoutStates;
        private FontInfo _copiedInfo;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var fontController = (FontController)target;

            // Init foldout states list.
            if (_foldoutStates == null)
            {
                _foldoutStates = new List<bool>();
                for (int i = 0; i < fontController.Count(); i++)
                {
                    _foldoutStates.Add(true);
                }
            }

            // Draw font settings.
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            for (int i = 0; i < fontController.Count(); i++)
            {
                DrawFontInfo(i);
                if (i < fontController.Count() - 1)
                {
                    EditorGUILayout.Separator();
                }
            }

            EditorGUILayout.Space();

            // Draw add button.
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                Undo.RecordObject(fontController, "Add culture");
                fontController.AddFontSetting();
                _foldoutStates.Add(true);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFontInfo(int i)
        {
            var fontController = (FontController)target;

            EditorGUILayout.BeginHorizontal();
            // Foldout.
            _foldoutStates[i] = EditorGUILayout.Foldout(_foldoutStates[i], string.IsNullOrEmpty(fontController.GetByIndex(i).Culture) ? "New Culture" : fontController.GetByIndex(i).Culture);

            // Copy.
            if(GUILayout.Button("Copy", GUILayout.Width(50), GUILayout.Height(15)))
            {
                _copiedInfo = fontController.GetByIndex(i);
            }
            // Paste.
            EditorGUI.BeginDisabledGroup(_copiedInfo == null);
            if(GUILayout.Button("Paste", GUILayout.Width(50), GUILayout.Height(15)))
            {
                Undo.RecordObject(fontController, "Paste culture");
                fontController.GetByIndex(i).CopyFontSizes(_copiedInfo);
            }
            EditorGUI.EndDisabledGroup();

            // Remove button.
            if (i != 0)
            {
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(15)))
                {
                    Undo.RecordObject(fontController, "Remove culture");
                    fontController.RemoveFontSettingAtIndex(i);
                    _foldoutStates.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!_foldoutStates[i])
                return;

            // Properties.
            var fontSettings = fontController.GetByIndex(i);
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(i == 0);
            fontSettings.Culture = EditorGUILayout.TextField("Culture", fontSettings.Culture);
            EditorGUI.EndDisabledGroup();
            fontSettings.Font = EditorGUILayout.ObjectField("Font", fontSettings.Font, typeof(TMPro.TMP_FontAsset), false) as TMPro.TMP_FontAsset;
            fontSettings.H1Size = EditorGUILayout.FloatField("H1 size", fontSettings.H1Size);
            fontSettings.H2Size = EditorGUILayout.FloatField("H2 size", fontSettings.H2Size);
            fontSettings.H3Size = EditorGUILayout.FloatField("H3 size", fontSettings.H3Size);
            fontSettings.BodySize = EditorGUILayout.FloatField("Body size", fontSettings.BodySize);
            fontSettings.SmallSize = EditorGUILayout.FloatField("Small size", fontSettings.SmallSize);
            fontSettings.IsRightToLeft = EditorGUILayout.Toggle("Right to left", fontSettings.IsRightToLeft);
            EditorGUILayout.EndVertical();
        }
    }
}