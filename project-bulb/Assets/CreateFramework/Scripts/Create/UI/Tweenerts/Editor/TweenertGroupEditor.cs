using UnityEditor;
using UnityEngine;

namespace Create.UI.Tweenerts
{
    [CustomEditor(typeof(TweenertGroup))]
    public class TweenertGroupEditor : Editor
    {
        private TweenertGroup _tweenertGroup;
        private SerializedObject _target;
        private SerializedProperty _statesInChildrenList;

        private void OnEnable()
        {
            _tweenertGroup = (TweenertGroup)target;
            _target = new SerializedObject(_tweenertGroup);

            _statesInChildrenList = _target.FindProperty("StatesInChildren");
        }

        public override void OnInspectorGUI()
        {
            _target.Update();

            if (_statesInChildrenList.arraySize == 0)
            {
                EditorGUILayout.LabelField("All states added to the 'AnimateTranform' component of this gameobject or a child gameobject will be displayed here.");
            }

            //foreach entry in stateslist: show properties
            for (int i = 0; i < _statesInChildrenList.arraySize; i++)
            {
                if (GUILayout.Button(string.Format("snap to '{0}'", _statesInChildrenList.GetArrayElementAtIndex(i).stringValue)))
                {
                    _tweenertGroup.SnapToState(_statesInChildrenList.GetArrayElementAtIndex(i).stringValue);
                }
            }
        }
    }
}