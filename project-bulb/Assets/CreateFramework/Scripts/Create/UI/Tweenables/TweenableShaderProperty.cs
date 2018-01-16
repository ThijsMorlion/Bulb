using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.Tweenables
{
    [RequireComponent(typeof(Tweenable))]
    public class TweenableShaderProperty : TweenableBase
    {
        public string ShaderProperty = "_Blend";

        [Tooltip("Leave blank for auto material assign.")]
        public Material Material;
        [SerializeField, HideInInspector]
        private float _inValue, _outValue;
        private float _fromValue;

        void Awake()
        {
            if(Material == null)
            {
                if (GetComponent<Renderer>() != null)
                {
                    Material = GetComponent<Renderer>().sharedMaterial;
                }
                else if (GetComponent<Graphic>() != null)
                {
                    Material = GetComponent<Graphic>().material;
                }
            }
        }

        public override void GetStartingValues(bool toIn)
        {
            _fromValue = toIn ? _outValue : _inValue;
        }

        public override void SetIn()
        {
            if (Material == null)
                return;

            _inStateSet = true;
            _inValue = Material.GetFloat(ShaderProperty);
        }

        public override void SetOut()
        {
            if (Material == null)
                return;

            _outStateSet = true;
            _outValue = Material.GetFloat(ShaderProperty);
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (Material == null)
                return;

            Material.SetFloat(ShaderProperty, Mathf.Lerp(_fromValue, toIn ? _inValue : _outValue, erpPos));
        }
    }
}