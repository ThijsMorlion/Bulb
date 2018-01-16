using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Create.UI.Tweenables
{
    [CustomEditor(typeof(Tweenable)), CanEditMultipleObjects]
    public class TweenableEditor : Editor
    {
        private static readonly string[] ExcludedDefaultProperties = new string[] { "m_Script", "OnStarted", "OnCompleted" };
        private static readonly string[] ExcludeAllExceptEvents = new string[] { "m_Script", "StartInOutState", "Delay", "DelayMode", "EaseType", "AnimateTransform", "AnimateColor" };

        public override void OnInspectorGUI()
        {
            // Draw duration.
            DrawDurationAndDurationByDistance();

            // Draw delay using regular inspector - uncomment and hide fields in inspector if delay mode should only be visible when delay > 0.
            //DrawDelayAndDelayMode();

            // Draw regular inspector.
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludedDefaultProperties);
            serializedObject.ApplyModifiedProperties();

            // Draw TweenableGroup related fields if only one object is selected.
            if (serializedObject.targetObjects.Length == 1)
            {
                var tweenable = (Tweenable)serializedObject.targetObject;
                if (tweenable.IsControlledByTweenableGroup)
                {
                    tweenable.NoDelayMirrorInTweenableGroup = EditorGUILayout.Toggle("Don't mirror delays", tweenable.NoDelayMirrorInTweenableGroup);
                    tweenable.IgnoreInTweenableGroup = EditorGUILayout.Toggle("Ignore in group", tweenable.IgnoreInTweenableGroup);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            // Draw movement animation related properties.
            DrawMovementMethodProperties();

            EditorGUILayout.Space();

            // Draw events.
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludeAllExceptEvents);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // Draw set state buttons.
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set in state"))
            {
                SetInState();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Set out state"))
            {
                SetOutState();
            }
            GUILayout.EndHorizontal();

            // Draw jump to state buttons. Only enable if a state has been set.
            GUILayout.BeginHorizontal();

            GUI.enabled = AreAllStatesSet(true);
            if (GUILayout.Button("Jump to in state"))
            {
                JumpToState(true);
            }

            GUILayout.Space(10);

            GUI.enabled = AreAllStatesSet(false);
            if (GUILayout.Button("Jump to out state"))
            {
                JumpToState(false);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDurationAndDurationByDistance()
        {
            // Draw the duration type selector.
            DrawDurationTypeEnum();

            // Decide whether to draw duration, duration by distance, or both.
            bool hasAnimateTransform = false, drawDuration = false, drawByDistance = false;
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if(tweenable.AnimateTransform)
                {
                    hasAnimateTransform = true;
                }
                if (tweenable.MovementDurationType == MovementDurationType.Time)
                {
                    drawDuration = true;
                }
                else if (tweenable.MovementDurationType == MovementDurationType.Distance)
                {
                    drawByDistance = true;
                }
            }

            if (drawDuration || !hasAnimateTransform)
            {
                DrawDuration();
            }
            if (drawByDistance && hasAnimateTransform)
            {
                DrawDurationByDistance();
            }
        }

        private void DrawDelayAndDelayMode()
        {
            DrawDelayField();
            DrawDelayModeEnum();
        }

        private void DrawDurationTypeEnum()
        {
            bool drawDurationType = false;
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (tweenable.AnimateTransform)
                {
                    drawDurationType = true;
                    break;
                }
            }

            if (!drawDurationType)
                return;

            List<MovementDurationType> durationTypes = new List<MovementDurationType>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (!durationTypes.Contains(tweenable.MovementDurationType))
                {
                    durationTypes.Add(tweenable.MovementDurationType);
                }
            }
            MovementDurationType newDurationType;
            if (DrawPossiblyMixedEnumProperty("Duration type", durationTypes, out newDurationType))
            {
                // Apply new value.
                foreach (var obj in serializedObject.targetObjects)
                {
                    var tweenable = (Tweenable)obj;
                    tweenable.MovementDurationType = newDurationType;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawDuration()
        {
            // Get current durations.
            List<float> durations = new List<float>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (!durations.Contains(tweenable.Duration))
                {
                    durations.Add(tweenable.Duration);
                }
            }

            // Draw field.
            float newDuration;
            bool durationChanged = DrawPossiblyMixedFloatProperty("Duration", durations, out newDuration);

            // Apply values.
            if (durationChanged)
            {
                foreach (var obj in serializedObject.targetObjects)
                {
                    var tweenable = (Tweenable)obj;
                    tweenable.Duration = newDuration;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawDurationByDistance()
        {
            // Get current values.
            List<float> unitsPerSecond = new List<float>();
            List<float> minDurations = new List<float>();
            List<float> maxDurations = new List<float>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (!unitsPerSecond.Contains(tweenable.UnitsPerSecond))
                {
                    unitsPerSecond.Add(tweenable.UnitsPerSecond);
                }
                if (!minDurations.Contains(tweenable.MinDuration))
                {
                    minDurations.Add(tweenable.MinDuration);
                }
                if (!maxDurations.Contains(tweenable.MaxDuration))
                {
                    maxDurations.Add(tweenable.MaxDuration);
                }
            }

            // Draw fields.
            float newUnitsPerSecond, newMinDuration, newMaxDuration;
            bool unitsPerSecondChanged = DrawPossiblyMixedFloatProperty("Units per second", unitsPerSecond, out newUnitsPerSecond);
            bool minDurationChanged = DrawPossiblyMixedFloatProperty("Min duration", minDurations, out newMinDuration);
            bool maxDurationChanged = DrawPossiblyMixedFloatProperty("Max duration", maxDurations, out newMaxDuration);

            // Apply values.
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (unitsPerSecondChanged)
                {
                    tweenable.UnitsPerSecond = newUnitsPerSecond;
                }
                if (minDurationChanged)
                {
                    tweenable.MinDuration = newMinDuration;
                }
                if (maxDurationChanged)
                {
                    tweenable.MaxDuration = newMaxDuration;
                }
            }
        }

        private void DrawDelayModeEnum()
        {
            bool drawDelayMode = false;
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (tweenable.Delay > 0)
                {
                    drawDelayMode = true;
                    break;
                }
            }

            if (!drawDelayMode)
                return;

            List<DelayModes> delayModes = new List<DelayModes>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (!delayModes.Contains(tweenable.DelayMode))
                {
                    delayModes.Add(tweenable.DelayMode);
                }
            }
            DelayModes newDelayMode;
            if (DrawPossiblyMixedEnumProperty("Delay mode", delayModes, out newDelayMode))
            {
                // Apply new value.
                foreach (var obj in serializedObject.targetObjects)
                {
                    var tweenable = (Tweenable)obj;
                    tweenable.DelayMode = newDelayMode;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawDelayField()
        {
            // Get current delays.
            List<float> delays = new List<float>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (!delays.Contains(tweenable.Delay))
                {
                    delays.Add(tweenable.Delay);
                }
            }

            // Draw field.
            float newDelay;
            bool delayChanged = DrawPossiblyMixedFloatProperty("Delay", delays, out newDelay);

            // Apply values.
            if (delayChanged)
            {
                if (newDelay < 0)
                    newDelay = 0;

                foreach (var obj in serializedObject.targetObjects)
                {
                    var tweenable = (Tweenable)obj;
                    tweenable.Delay = newDelay;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawMovementMethodProperties()
        {
            // Draw movement type enum if any of the selected objects are set to animatie movement.
            bool drawMovementTypeEnum = false;
            List<MovementMethod> movementMethods = new List<MovementMethod>();
            List<float> sinScales = new List<float>();
            foreach (var obj in serializedObject.targetObjects)
            {
                var tweenable = (Tweenable)obj;
                if (tweenable.AnimateTransform)
                {
                    drawMovementTypeEnum = true;
                    if (!movementMethods.Contains(tweenable.MovementMethod))
                    {
                        movementMethods.Add(tweenable.MovementMethod);
                    }
                    if (!sinScales.Contains(tweenable.SinusodalScale))
                    {
                        sinScales.Add(tweenable.SinusodalScale);
                    }
                }
            }

            if (!drawMovementTypeEnum)
                return;

            // Draw movement method selector.
            MovementMethod newMethod;
            bool movementTypeChanged = DrawPossiblyMixedEnumProperty("Movement method", movementMethods, out newMethod);

            // Draw sinusodal scale selector if applicable.
            float newSinScale = 0;
            bool sinScaleChanged = false;
            if (newMethod == MovementMethod.Sinusodal)
            {
                sinScaleChanged = DrawPossiblyMixedFloatProperty("Sinusodal scale", sinScales, out newSinScale);
            }

            foreach (var obj in serializedObject.targetObjects)
            {
                // Apply values.
                Tweenable tweenable = (Tweenable)obj;
                var serializedObj = new SerializedObject(tweenable.gameObject);
                if (movementTypeChanged)
                {
                    tweenable.MovementMethod = newMethod;
                }
                if (sinScaleChanged)
                {
                    tweenable.SinusodalScale = newSinScale;
                }

                serializedObj.ApplyModifiedProperties();
            }
        }

        private bool DrawPossiblyMixedFloatProperty(string label, List<float> current, out float result)
        {
            EditorGUI.showMixedValue = current.Count > 1;
            EditorGUI.BeginChangeCheck();
            result = EditorGUILayout.FloatField(label, current[0]);
            bool change = EditorGUI.EndChangeCheck();
            EditorGUI.showMixedValue = false;

            return change;
        }

        private bool DrawPossiblyMixedEnumProperty<T>(string label, List<T> current, out T result) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumeration.");

            EditorGUI.showMixedValue = current.Count > 1;
            EditorGUI.BeginChangeCheck();
            result = (T)Convert.ChangeType(EditorGUILayout.EnumPopup(label, (Enum)Convert.ChangeType(current[0], typeof(T))), typeof(T));
            bool change = EditorGUI.EndChangeCheck();
            EditorGUI.showMixedValue = false;

            return change;
        }

        private bool AreAllStatesSet(bool inState)
        {
            //Check all selected objects.
            foreach (UnityEngine.Object obj in serializedObject.targetObjects)
            {
                //Check all tweenables.
                TweenableBase[] tweenables = ((MonoBehaviour)obj).GetComponents<TweenableBase>();
                foreach (TweenableBase tweenable in tweenables)
                {
                    if (inState && !tweenable.InStateSet)
                        return false;
                    else if (!inState && !tweenable.OutStateSet)
                        return false;
                }
            }

            return true;
        }

        private void SetInState()
        {
            foreach (UnityEngine.Object obj in serializedObject.targetObjects)
            {
                MonoBehaviour component = (MonoBehaviour)obj;
                var serializedObj = new SerializedObject(component.gameObject);
                //Set in state in any other tweenable components.
                TweenableBase[] tweenables = component.GetComponents<TweenableBase>();
                foreach (TweenableBase tweenable in tweenables)
                {
                    tweenable.SetIn();
                }

                serializedObj.ApplyModifiedProperties();
            }
        }

        private void SetOutState()
        {
            foreach (UnityEngine.Object obj in serializedObject.targetObjects)
            {
                MonoBehaviour component = (MonoBehaviour)obj;
                var serializedObj = new SerializedObject(component.gameObject);

                //Set out state in all tweenable components.
                TweenableBase[] tweenables = component.GetComponents<TweenableBase>();
                foreach (TweenableBase tweenable in tweenables)
                {
                    tweenable.SetOut();
                }

                serializedObj.ApplyModifiedProperties();
            }
        }

        private void JumpToState(bool toIn)
        {
            foreach (UnityEngine.Object obj in serializedObject.targetObjects)
            {
                MonoBehaviour component = (MonoBehaviour)obj;

                //Jump to in state in all tweenables.
                TweenableBase[] tweenables = component.GetComponents<TweenableBase>();
                foreach (TweenableBase tweenable in tweenables)
                {
                    tweenable.Snap(toIn);
                }
            }

            EditorUtility.SetDirty(target);
        }
    }
}