using UnityEditor;

namespace Create.UI.Tweenables
{
    [CustomEditor(typeof(TweenableMaterialColor))]
    public class TweenableMaterialColorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TweenableMaterialColor tweenable = (TweenableMaterialColor)serializedObject.targetObject;
            if (tweenable.IsInMaterialFadeGroup)
            {
                tweenable.IgnoreInMaterialFade = EditorGUILayout.Toggle("Ignore in material fade", tweenable.IgnoreInMaterialFade);
            }
        }
    }
}