using UnityEngine;

namespace Create.UI.Tweenables
{
    [RequireComponent(typeof(Tweenable))]
    public class TweenableMaterialColor : TweenableBase
    {
        public Color Color;
        public string ShaderColorName = "_Color";
        [HideInInspector]
        public bool IgnoreInMaterialFade;
        public bool IsInMaterialFadeGroup { get; private set; }

        [SerializeField, HideInInspector]
        private Color _inColor, _outColor;
        private Color _startColor;

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;

        void OnEnable()
        {
            // Init.
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            IsInMaterialFadeGroup = GetComponentInParent<TweenableSharedMaterialFade>() != null;
        }

        void OnValidate()
        {
            if (_renderer == null || _propertyBlock == null)
                return;

            // Apply color to prop block.
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(ShaderColorName, Color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public override void GetStartingValues(bool toIn)
        {
            if (_renderer == null)
                return;

            _startColor = toIn ? _outColor : _inColor;
        }

        public override void SetIn()
        {
            // Make sure we have a renderer, try to get it otherwise.
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (_renderer == null)
                return;

            // Fetch material properties.
            _renderer.GetPropertyBlock(_propertyBlock);
            // Fetch color.
            _inColor = _propertyBlock.GetVector(ShaderColorName);

            _inStateSet = true;
        }

        public override void SetOut()
        {
            // Make sure we have a renderer, try to get it otherwise.
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (_renderer == null)
                return;

            // Fetch material properties.
            _renderer.GetPropertyBlock(_propertyBlock);
            // Fetch color.
            _outColor = _propertyBlock.GetVector(ShaderColorName);

            _outStateSet = true;
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            // Make sure we have a renderer, try to get it otherwise.
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (_renderer == null)
                return;
            if(_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            // Fetch current properties.
            _renderer.GetPropertyBlock(_propertyBlock);
            Color currentColor = _propertyBlock.GetVector(ShaderColorName);

            bool isAlphaAllowed = true;

            // If we are in a fade group, check if we are allowed to change the alpha.
            if (IsInMaterialFadeGroup && !IgnoreInMaterialFade)
            {
                isAlphaAllowed = false;
            }

            // Blend the color.
            Color targetColor = Color.Lerp(_startColor, toIn ? _inColor : _outColor, erpPos);
            // Reset alpha if changing it is not allowed.
            if (!isAlphaAllowed)
            {
                targetColor.a = currentColor.a;
            }

            Color = targetColor;

            // Apply the color.
            _propertyBlock.SetVector(ShaderColorName, targetColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}