using UnityEditor;
using UnityEngine;

namespace Create.Localization.Editor
{
    [CustomEditor(typeof(XmlLocalizationLoader))]
    public class XmlLanguageControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if(GUILayout.Button("Create dummy xmls"))
            {
                ((XmlLocalizationLoader)target).CreateDummyXmls();
            }
        }
    }
}