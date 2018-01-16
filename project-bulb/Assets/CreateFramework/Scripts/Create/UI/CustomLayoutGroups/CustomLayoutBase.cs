using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.CustomLayoutGroups
{
    public abstract class CustomLayoutBase : MonoBehaviour
    {
        public float Spacing;
        public RectOffset Padding;
        public bool SizeToContent = true;

        private float _layoutScalar = 1;
        /// <summary>
        /// Scales all values which determine the child elements positions. Mainly used to easily animate the layout group with Tweenable.
        /// </summary>
        public float LayoutScalar
        {
            get { return _layoutScalar; }
            set
            {
                if (_layoutScalar == value)
                    return;
                _layoutScalar = value;
                UpdateLayout();
            }
        }

        protected LayoutElement _layoutElement;
        protected RectTransform _rect;
        protected int _previousChildCount;
        protected float[] _previousSizes;
        protected LayoutElement[] _childLayoutElements;
        protected RectTransform[] _childRects;
        protected float _previousSpacing;

        protected virtual void OnEnable()
        {
            _layoutElement = GetComponent<LayoutElement>();
            _rect = GetComponent<RectTransform>();

            UpdateChildInfo();
            UpdateLayout();
        }

        protected virtual void Update()
        {
            // Update layout from child count.
            if (transform.childCount != _previousChildCount)
            {
                UpdateChildInfo();
                UpdateLayout();
                return;
            }

            // Update layout from size change.
            for (int i = 0; i < transform.childCount; i++)
            {
                if (_childLayoutElements[i] != null && _previousSizes[i] != _childLayoutElements[i].preferredHeight
                    || _childLayoutElements[i] == null && _previousSizes[i] != 0)
                {
                    UpdateLayout();
                    return;
                }
            }

            // Update from spacing change.
            if (_previousSpacing != Spacing)
            {
                UpdateLayout();
                return;
            }
        }

        protected abstract void UpdateLayout();

        protected virtual void UpdateChildInfo()
        {
            _previousChildCount = transform.childCount;
            _previousSizes = new float[_previousChildCount];
            _childLayoutElements = new LayoutElement[_previousChildCount];
            _childRects = new RectTransform[_previousChildCount];

            for (int i = 0; i < _childLayoutElements.Length; i++)
            {
                var child = transform.GetChild(i);
                _childLayoutElements[i] = child.GetComponent<LayoutElement>();
                _childRects[i] = child.GetComponent<RectTransform>();

                // Enforce anchorage.
                _childRects[i].anchorMin = _childRects[i].anchorMax = Vector2.zero;
            }
        }
    }
}