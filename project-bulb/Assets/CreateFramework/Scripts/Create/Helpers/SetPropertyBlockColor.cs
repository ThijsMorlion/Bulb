using UnityEngine;

namespace Create.Helpers
{
    /// <summary>
    /// Sets the color on all child renderes using the property block.
    /// </summary>
    [ExecuteInEditMode]
    public class SetPropertyBlockColor : MonoBehaviour
    {
        public Color Color;
        public string ShaderColorName = "_Color";

        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;

        void OnEnable()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            SetColors();
        }

        void OnValidate()
        {
            SetColors();
        }

        private void SetColors()
        {
            if (_renderers == null || _propertyBlock == null)
                return;

            foreach (var renderer in _renderers)
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ShaderColorName, Color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}