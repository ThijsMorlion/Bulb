using System;
using UnityEngine;
using UnityEditor;

namespace Create.UI.Tweenerts
{
    public abstract class BaseTweenertOnPageStateEditor<T> : Editor where T : struct
    {
        private BaseTweenertOnPageState<T> _tweenertOnPageState;
        private SerializedObject _target;
        private SerializedProperty _tweenertStateNames;
        private SerializedProperty _goToOutStateOnUnsetPage;
        private SerializedProperty _areAllAnimationEqualDuration;
        private SerializedProperty _allAnimationsDuration;
        private SerializedProperty _pageStates;

        private void OnEnable()
        {
            _tweenertOnPageState = (BaseTweenertOnPageState<T>)target;
            _target = new SerializedObject(_tweenertOnPageState);

            _tweenertStateNames = _target.FindProperty("TweenertStateNames");
            _goToOutStateOnUnsetPage = _target.FindProperty("GoToOutStateOnUnusedPage");
            _areAllAnimationEqualDuration = _target.FindProperty("AreAllAnimationEqualDuration");
            _allAnimationsDuration = _target.FindProperty("AllAnimationsDuration");
            _pageStates = _target.FindProperty("PageStates");
        }

        public override void OnInspectorGUI()
        {
            //Update the list
            _target.Update();
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_areAllAnimationEqualDuration);
            if (_areAllAnimationEqualDuration.boolValue)
            {
                EditorGUILayout.PropertyField(_allAnimationsDuration);
                EditorGUILayout.Space();
            }

            //display GoToOutPositionOnUnsetPage checkbox
            EditorGUILayout.PropertyField(_goToOutStateOnUnsetPage);
            if (_goToOutStateOnUnsetPage.boolValue)
            {
                string[] options = new string[_tweenertStateNames.arraySize];
                for (int j = 0; j < _tweenertStateNames.arraySize; j++)
                {
                    options[j] = _tweenertStateNames.GetArrayElementAtIndex(j).stringValue;
                }

                int selectedTweenertStateIndex = GetOutTweenertStateIndex();
                selectedTweenertStateIndex = EditorGUILayout.Popup("Out State", selectedTweenertStateIndex, options);

                if (selectedTweenertStateIndex >= 0)
                {
                    SetSelectedOutState(options[selectedTweenertStateIndex]);
                }
            }

            EditorGUILayout.Space();

            //display each item of the list in the editor
            for (int i = 0; i < _pageStates.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("Box");

                //draw do nothing bool
                bool newDoNothing = false;
                bool doNothingChanged = false;
                doNothingChanged = DrawDoNothing(((PageState)_pageStates.GetArrayElementAtIndex(i)).DoNothing, out newDoNothing);
                if (doNothingChanged)
                {
                    _tweenertOnPageState.PageStates[i].DoNothing = newDoNothing;
                }

                //draw on page
                T newPage;
                if (DrawPagesEnum((T)Enum.ToObject(typeof(T), ((PageState)_pageStates.GetArrayElementAtIndex(i)).OnPage), out newPage))
                {
                    _tweenertOnPageState.PageStates[i].OnPage = (int)Enum.ToObject(typeof(T), newPage);
                }

                if (!((PageState)_pageStates.GetArrayElementAtIndex(i)).DoNothing)
                {
                    //draw go to state
                    string[] tweenertStateNameOptions = new string[_tweenertStateNames.arraySize];
                    for (int j = 0; j < _tweenertStateNames.arraySize; j++)
                    {
                        tweenertStateNameOptions[j] = _tweenertStateNames.GetArrayElementAtIndex(j).stringValue;
                    }

                    if (tweenertStateNameOptions.Length > 0)
                    {
                        int selectedTweenertStateIndex = GetSelectedTweenertStateIndexForPageState(i);
                        EditorGUI.BeginChangeCheck();
                        selectedTweenertStateIndex = EditorGUILayout.Popup("Go to state", selectedTweenertStateIndex, tweenertStateNameOptions);
                        if (EditorGUI.EndChangeCheck() && selectedTweenertStateIndex >= 0)
                        {
                            SetSelectedStateForStageOnPage(i, tweenertStateNameOptions[selectedTweenertStateIndex]);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Add states to the Tweenert component first.");
                    }

                    if (!_areAllAnimationEqualDuration.boolValue)
                    {
                        //draw duration float
                        float newDuration = 0;
                        bool durationChanged = false;
                        durationChanged = DrawDuration(((PageState)_pageStates.GetArrayElementAtIndex(i)).Duration, out newDuration);
                        if (durationChanged)
                        {
                            _tweenertOnPageState.PageStates[i].Duration = newDuration;
                        }
                    }
                }

                EditorGUILayout.Space();

                //remove button
                if (GUILayout.Button("- Remove step", GUILayout.Width(110)))
                {
                    Undo.RegisterCompleteObjectUndo(_tweenertOnPageState.gameObject, "Page state removed");
                    _pageStates.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            //add new Animationstep button
            if (GUILayout.Button("+ Add Pagestate"))
            {
                Undo.RegisterCompleteObjectUndo(_tweenertOnPageState.gameObject, "Add Tweenert page");
                _pageStates.InsertArrayElementAtIndex(_pageStates.arraySize);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            //apply the changes to our list
            _target.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected bool DrawPagesEnum(T currentPage, out T newPage)
        {
            EditorGUI.BeginChangeCheck();
            newPage = (T)Enum.ToObject(typeof(T), EditorGUILayout.EnumPopup("On Page", (Enum)Enum.ToObject(typeof(T), currentPage)));
            return EditorGUI.EndChangeCheck();
        }

        private bool DrawDoNothing(bool originalValue, out bool result)
        {
            EditorGUI.BeginChangeCheck();
            result = EditorGUILayout.Toggle("Do nothing", originalValue);
            return EditorGUI.EndChangeCheck();
        }

        private bool DrawDuration(float originalDuration, out float result)
        {
            EditorGUI.BeginChangeCheck();
            result = EditorGUILayout.FloatField("Duration", originalDuration);
            return EditorGUI.EndChangeCheck();
        }

        private int GetSelectedTweenertStateIndexForPageState(int pageStateIndex)
        {
            if (_tweenertOnPageState.PageStates.Count <= pageStateIndex)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(_tweenertOnPageState.PageStates[pageStateIndex].StateName))
            {
                return -1;
            }

            return _tweenertOnPageState.TweenertStateNames.IndexOf(_tweenertOnPageState.PageStates[pageStateIndex].StateName);
        }

        private void SetSelectedStateForStageOnPage(int pageStateIndex, string selectedTweenertStateName)
        {
            _tweenertOnPageState.PageStates[pageStateIndex].StateName = selectedTweenertStateName;
        }

        private void SetSelectedOutState(string outSateName)
        {
            _tweenertOnPageState.OutStateName = outSateName;
        }

        private int GetOutTweenertStateIndex()
        {
            if (string.IsNullOrEmpty(_tweenertOnPageState.OutStateName))
            {
                return -1;
            }

            return _tweenertOnPageState.TweenertStateNames.IndexOf(_tweenertOnPageState.OutStateName);
        }
    }
}