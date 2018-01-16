using UnityEditor;
using UnityEngine.UI;

namespace Create.UI.Tweenables
{
    [CustomEditor(typeof(TweenableCanvasGroup))]
    public class TweenableCanvasGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TweenableCanvasGroup canvasGroup = (TweenableCanvasGroup)serializedObject.targetObject;
            canvasGroup.DisableInteractivity = EditorGUILayout.Toggle("Disable interactivity", canvasGroup.DisableInteractivity);
            canvasGroup.DisableScripts = EditorGUILayout.Toggle("Disable scripts", canvasGroup.DisableScripts);

            // Show ignore in layout toggle if there is a layout element component.
            var layoutElement = canvasGroup.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                canvasGroup.IgnoreInLayout = EditorGUILayout.Toggle("Ignore in layout", canvasGroup.IgnoreInLayout);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}