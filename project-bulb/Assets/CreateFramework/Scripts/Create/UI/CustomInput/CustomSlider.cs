using UnityEngine;

namespace Create.UI.CustomInput
{
    public class CustomSlider : MonoBehaviour
    {
        public float Min;
        public float Max = 1;

        public RectTransform SlidingArea, Handle;

        private float _value;
        public float Value
        {
            get { return _value; }
        }

        private float _previousHandlePosition, _initialY;

        void OnValidate()
        {
            if (Max < Min)
            {
                Max = Min;
            }
        }

        void Awake()
        {
            if(Handle != null)
            {
                _initialY = Handle.anchoredPosition.y;
            }
        }

        void Update()
        {
            if (SlidingArea == null || Handle == null)
                return;

            if(!Mathf.Approximately(Handle.anchoredPosition.x, _previousHandlePosition))
            {
                _previousHandlePosition = Handle.anchoredPosition.x;
                _value = Handle.anchoredPosition.x / SlidingArea.rect.width * (Max - Min);
            }
        }

        public void SetValue(float value)
        {
            if (_value == value)
                return;

            value = Mathf.Clamp(value, Min, Max);
            _value = value;

            if (Handle != null && SlidingArea != null)
            {
                Handle.anchoredPosition = new Vector2(value / (Max - Min) * SlidingArea.rect.width, _initialY);
            }
        }
    }
}