using UnityEditor;
using UnityEngine;

namespace Create.UI.Tweenerts
{
    [CustomEditor(typeof(Tweenert))]
    public class TweenertEditor : Editor
    {
        private Tweenert _animateTransform;
        private SerializedObject _target;
        private SerializedProperty _animateTransformStatesList;
        private SerializedProperty _animateTransformStatesInParentsList;
        private SerializedProperty _animationCurve;
        private SerializedProperty _ignoreInGroup;

        private void OnEnable()
        {
            _animateTransform = (Tweenert)target;
            _target = new SerializedObject(_animateTransform);

            _animateTransformStatesList = _target.FindProperty("States");
            _animateTransformStatesInParentsList = _target.FindProperty("StatesInParents");
            _animationCurve = _target.FindProperty("AnimationCurve");
            _ignoreInGroup = _target.FindProperty("IgnoreInGroup");
        }

        public override void OnInspectorGUI()
        {
            _target.Update();

            //show combobox to choose animationcurve
            EditorGUILayout.PropertyField(_animationCurve);

            //show checkbox to ignore component in group
            EditorGUILayout.PropertyField(_ignoreInGroup);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //foreach entry in stateslist: show properties
            for (int i = 0; i < _animateTransformStatesList.arraySize; i++)
            {
                //get properties
                SerializedProperty listItemRef = _animateTransformStatesList.GetArrayElementAtIndex(i);
                SerializedProperty stateName = listItemRef.FindPropertyRelative("StateName");
                SerializedProperty animateAnchoredPosition = listItemRef.FindPropertyRelative("AnimateAnchoredPosition");
                SerializedProperty anchoredPosition = listItemRef.FindPropertyRelative("AnchoredPosition");
                SerializedProperty animateLocalScale = listItemRef.FindPropertyRelative("AnimateLocalScale");
                SerializedProperty localScale = listItemRef.FindPropertyRelative("LocalScale");
                SerializedProperty animateSizeDelta = listItemRef.FindPropertyRelative("AnimateSizeDelta");
                SerializedProperty sizeDelta = listItemRef.FindPropertyRelative("SizeDelta");
                SerializedProperty animateRotation = listItemRef.FindPropertyRelative("AnimateRotation");
                SerializedProperty rotation = listItemRef.FindPropertyRelative("Rotation");
                SerializedProperty animateAlpha = listItemRef.FindPropertyRelative("AnimateAlpha");
                SerializedProperty alpha = listItemRef.FindPropertyRelative("Alpha");

                //start display of list entry
                EditorGUILayout.BeginVertical("Box");

                //display name property
                EditorGUILayout.PropertyField(stateName);

                //only when the name is empty: suggest the statenames already defined on parents. Usefull when creating TweenertGroups
                if (string.IsNullOrEmpty(stateName.stringValue) && IsAnyStateFromParentsNotYetUsed() && !_ignoreInGroup.boolValue)
                {
                    EditorGUILayout.LabelField("Or get name from parents:");

                    for (int j = 0; j < _animateTransformStatesInParentsList.arraySize; j++)
                    {
                        string parentStateName = _animateTransformStatesInParentsList.GetArrayElementAtIndex(j).stringValue;

                        //don't show button is statename is empty or already defined on component
                        if (!string.IsNullOrEmpty(parentStateName) && !IsStateNameUsedOnComponent(parentStateName))
                        {
                            if (GUILayout.Button(parentStateName))
                            {
                                _animateTransform.ChangeStateName(i, parentStateName);
                            }
                        }
                    }

                    EditorGUILayout.Space();
                }

                //only display properties when name is entered
                if (!string.IsNullOrEmpty(stateName.stringValue))
                {
                    if (GUILayout.Button("snap to state"))
                    {
                        //anchoredposition
                        if (animateAnchoredPosition.boolValue)
                        {
                            _animateTransform.SnapToStateListItemComponentValues(Tweenert.TransformComponents.anchoredPosition, i);
                        }

                        //localscale
                        if (animateLocalScale.boolValue)
                        {
                            _animateTransform.SnapToStateListItemComponentValues(Tweenert.TransformComponents.localScale, i);
                        }

                        //sizedelta
                        if (animateSizeDelta.boolValue)
                        {
                            _animateTransform.SnapToStateListItemComponentValues(Tweenert.TransformComponents.sizeDelta, i);
                        }

                        //rotation
                        if (animateRotation.boolValue)
                        {
                            _animateTransform.SnapToStateListItemComponentValues(Tweenert.TransformComponents.rotation, i);
                        }

                        //alpha
                        if (animateAlpha.boolValue)
                        {
                            _animateTransform.SnapToStateListItemComponentValues(Tweenert.TransformComponents.alpha, i);
                        }
                    }

                    EditorGUILayout.Space();

                    //anchoredposition
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(animateAnchoredPosition);
                    if (animateAnchoredPosition.boolValue)
                    {
                        EditorGUILayout.PropertyField(anchoredPosition, GUIContent.none);
                        ShowInteractionButtons(Tweenert.TransformComponents.anchoredPosition, i);
                    }
                    GUILayout.EndHorizontal();

                    //localscale
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(animateLocalScale);
                    if (animateLocalScale.boolValue)
                    {
                        EditorGUILayout.PropertyField(localScale, GUIContent.none);
                        ShowInteractionButtons(Tweenert.TransformComponents.localScale, i);
                    }
                    GUILayout.EndHorizontal();

                    //sizedelta
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(animateSizeDelta);
                    if (animateSizeDelta.boolValue)
                    {
                        EditorGUILayout.PropertyField(sizeDelta, GUIContent.none);
                        ShowInteractionButtons(Tweenert.TransformComponents.sizeDelta, i);
                    }
                    GUILayout.EndHorizontal();

                    //rotation
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(animateRotation);
                    if (animateRotation.boolValue)
                    {
                        EditorGUILayout.PropertyField(rotation, GUIContent.none);
                        ShowInteractionButtons(Tweenert.TransformComponents.rotation, i);
                    }
                    GUILayout.EndHorizontal();

                    //alpha
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(animateAlpha);
                    if (animateAlpha.boolValue)
                    {
                        EditorGUILayout.PropertyField(alpha, GUIContent.none);
                        ShowInteractionButtons(Tweenert.TransformComponents.alpha, i);
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }

                //remove button
                if (GUILayout.Button("- Remove State", GUILayout.Width(110)))
                {
                    _animateTransformStatesList.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            //add new Animationstep button
            if (GUILayout.Button("+ Add State"))
            {
                _animateTransform.States.Add(new Tweenert.State());
            }

            EditorGUILayout.Space();

            //Apply the changes to the list
            _target.ApplyModifiedProperties();
        }
        private void ShowInteractionButtons(Tweenert.TransformComponents component, int listIndex)
        {
            if (GUILayout.Button("copy current", GUILayout.Width(85)))
            {
                _animateTransform.CopyCurrentComponentValuesToStateListItem(component, listIndex);
            }

            if (GUILayout.Button("snap", GUILayout.Width(40)))
            {
                _animateTransform.SnapToStateListItemComponentValues(component, listIndex);
            }
        }

        private bool IsStateNameUsedOnComponent(string parentStateName)
        {
            foreach (var state in _animateTransform.States)
            {
                if (state.StateName == parentStateName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAnyStateFromParentsNotYetUsed()
        {
            foreach (var parentstate in _animateTransform.StatesInParents)
            {
                if (!IsStateNameUsedOnComponent(parentstate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
