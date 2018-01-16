using Create.UI.Tweenables;
using UnityEngine;

namespace Create.UI.CustomLayoutGroups
{
    [RequireComponent(typeof(Tweenable)), RequireComponent(typeof(CustomLayoutBase))]
    public class TweenableCustomLayoutScalar : TweenableBase
    {
        [SerializeField, HideInInspector]
        private float _inScalar, _outScalar;
        private CustomLayoutBase _customLayout;

        void OnEnable()
        {
            if (_customLayout == null)
                _customLayout = GetComponent<CustomLayoutBase>();

            _inStateSet = _outStateSet = true;
            _inScalar = 1;
            _outScalar = 0;
        }

        public override void GetStartingValues(bool toIn)
        {
            
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (_customLayout == null)
                _customLayout = GetComponent<CustomLayoutBase>();

            _customLayout.LayoutScalar = Mathf.Lerp(toIn ? _outScalar : _inScalar, toIn ? _inScalar : _outScalar, erpPos);
        }

        public override void SetIn()
        {
     
        }

        public override void SetOut()
        {
           
        }
    }
}