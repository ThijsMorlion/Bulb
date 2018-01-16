using UnityEngine;
using UnityEditor;

namespace Create.UI.Tweenables
{
    [CustomEditor(typeof(TweenableGroup))]
    public class TweenableGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Jump to in state"))
            {
                JumpToState(true);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Jump to out state"))
            {
                JumpToState(false);
            }
            GUILayout.EndHorizontal();
        }

        private void JumpToState(bool toIn)
        {
            foreach (UnityEngine.Object obj in serializedObject.targetObjects)
            {
                MonoBehaviour component = (MonoBehaviour)obj;
                Tweenable[] tweenables = component.GetComponentsInChildren<Tweenable>();
                foreach (var tweenable in tweenables)
                    tweenable.Snap(toIn);
            }

            EditorUtility.SetDirty(target);
        }
    }
}