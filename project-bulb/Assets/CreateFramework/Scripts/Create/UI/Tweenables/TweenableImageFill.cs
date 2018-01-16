using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.Tweenables
{
    [RequireComponent(typeof(Tweenable)), RequireComponent(typeof(Image))]
    public class TweenableImageFill : TweenableBase
    {
        private Image _image;

        [SerializeField, HideInInspector]
        private float _inAmount, _outAmount;

        void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            // Default states.
            if (_inAmount == 0 && _outAmount == 0)
            {
                _inAmount = 1;
                _outAmount = 0;
                _inStateSet = _outStateSet = true;
            }
        }

        public override void GetStartingValues(bool toIn)
        {
        }

        public override void SetIn()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            _inAmount = _image.fillAmount;
            _inStateSet = true;
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (_image == null)
                _image = GetComponent<Image>();

            _image.fillAmount = Mathf.Lerp(toIn ? _outAmount : _inAmount, toIn ? _inAmount : _outAmount, erpPos);
        }

        public override void SetOut()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            _outAmount = _image.fillAmount;
            _outStateSet = true;
        }
    }
}