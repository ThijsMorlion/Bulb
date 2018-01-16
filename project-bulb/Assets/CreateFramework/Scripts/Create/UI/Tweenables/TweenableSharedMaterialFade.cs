using UnityEngine;

namespace Create.UI.Tweenables
{
    /// <summary>
    /// Changes the alpha value of all underlying Renderers using material property blocks, so it can cooperate with TweenableMaterialColor.
    /// </summary>
    [RequireComponent(typeof(Tweenable))]
    public class TweenableSharedMaterialFade : TweenableBase
    {
        [Range(0, 1)]
        public float Alpha;
        public string ShaderAlphaName = "_Alpha";

        [SerializeField, HideInInspector]
        private float _inAlpha, _outAlpha;
        private float _startAlpha;
        private Renderer[] _renderers;

        private MaterialPropertyBlock _sharedPropBlock;

        void OnEnable()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _sharedPropBlock = new MaterialPropertyBlock();
        }

        void OnValidate()
        {
            if (_renderers == null || _sharedPropBlock == null)
                return;

            // Update property block alphas to slider value in editor.
            foreach (var renderer in _renderers)
            {
                SetAlpha(renderer);
            }
        }

        public override void GetStartingValues(bool toIn)
        {
            if (_renderers == null || _renderers.Length == 0)
                return;

            _startAlpha = toIn ? _outAlpha : _inAlpha;
        }

        public override void SetIn()
        {
            if (_renderers == null || _renderers.Length == 0)
                return;

            // Set in alpha.
            _inAlpha = Alpha;
            _inStateSet = true;
        }

        public override void SetOut()
        {
            if (_renderers == null || _renderers.Length == 0)
                return;

            // Set out alpha.
            _outAlpha = Alpha;
            _outStateSet = true;
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (_renderers == null || _renderers.Length == 0)
                return;

            // Update alpha value so it is synced in the editor.
            Alpha = Mathf.Lerp(_startAlpha, toIn ? _inAlpha : _outAlpha, erpPos);

            // Update alpha values in all renderers.
            foreach (var renderer in _renderers)
            {
                SetAlpha(renderer);
            }
        }

        private void SetAlpha(Renderer renderer)
        {
            // Don't change alpha on elements which are set to ignore this group.
            var tweenableMatColor = renderer.GetComponent<TweenableMaterialColor>();
            if (tweenableMatColor != null && tweenableMatColor.IgnoreInMaterialFade)
                return;

            renderer.GetPropertyBlock(_sharedPropBlock);
            _sharedPropBlock.SetFloat(ShaderAlphaName, Alpha);
            renderer.SetPropertyBlock(_sharedPropBlock);
        }
    }
}