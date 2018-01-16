using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.Tweenables
{
    [RequireComponent(typeof(Tweenable))]
    public class TweenableToPreferredSize : TweenableBase
    {
        [Tooltip("The target layout group used to determine the preferred size of the element.")]
        public LayoutGroup Target;
        [Tooltip("Leave blank for attached layout element.")]
        public LayoutElement TargetLayoutElement;
        public float Padding;

        private float _fromSize, _toSize;
        private float _collapsedSize, _previousPreferredSize;
        private bool _isInited;
        private Tweenable _tweenable;

        void OnEnable()
        {
            _tweenable = GetComponent<Tweenable>();
            _inStateSet = true;

            if (TargetLayoutElement == null)
                TargetLayoutElement = GetComponent<LayoutElement>();
        }

        void Start()
        {
            RunInitPreferredHeight(!_tweenable.StartInOutState);
        }

        void Update()
        {
            // Wait for the content to init before doing runtime size change checks.
            if (!_isInited)
                return;

            // If the content changes size, also animate to the new size.
            if (Target is VerticalLayoutGroup)
            {
                if (_previousPreferredSize != Target.preferredHeight)
                {
                    _previousPreferredSize = Target.preferredHeight;

                    // Force tweenable to update.
                    if (_tweenable.State != TweenableState.BetweenToIn && _tweenable.State != TweenableState.BetweenToOut)
                    {
                        _tweenable.TweenToState(_tweenable.State == TweenableState.In);
                    }
                }
            }
            else if (Target is HorizontalLayoutGroup)
            {
                if (_previousPreferredSize != Target.preferredWidth)
                {
                    _previousPreferredSize = Target.preferredWidth;

                    // Force tweenable to update.
                    if (_tweenable.State != TweenableState.BetweenToIn && _tweenable.State != TweenableState.BetweenToOut)
                    {
                        _tweenable.TweenToState(_tweenable.State == TweenableState.In);
                    }
                }
            }
        }

        public override void GetStartingValues(bool toIn)
        {
            if (toIn)
            {
                _fromSize = _collapsedSize;
                _toSize = CalculateTargetPreferredSize();
            }
            else
            {
                _fromSize = CalculateTargetPreferredSize();
                _toSize = _collapsedSize;
            }
        }

        public override void SetIn()
        {
            _inStateSet = true;
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (!Application.isPlaying || _fromSize == 0 && _toSize == 0)
            {
                GetStartingValues(toIn);
            }

            if (Target == null || TargetLayoutElement == null)
                return;

            if(Target is VerticalLayoutGroup)
            {
                TargetLayoutElement.preferredHeight = Mathf.Lerp(_fromSize, _toSize, erpPos);
            }
            else if(Target is HorizontalLayoutGroup)
            {
                TargetLayoutElement.preferredWidth = Mathf.Lerp(_fromSize, _toSize, erpPos);
            }
        }

        public override void SetOut()
        {
            _outStateSet = true;
            if (TargetLayoutElement == null)
                TargetLayoutElement = GetComponent<LayoutElement>();

            if (Target is VerticalLayoutGroup)
            {
                _collapsedSize = TargetLayoutElement.preferredHeight;
            }
            else if (Target is HorizontalLayoutGroup)
            {
                _collapsedSize = TargetLayoutElement.preferredWidth;
            }
        }

        private IEnumerator RunInitPreferredHeight(bool isExpanded)
        {
            if(Target == null)
                yield break;

            yield return null;
            
            if(Target is VerticalLayoutGroup)
            {
                TargetLayoutElement.preferredHeight = isExpanded ? Target.preferredHeight + _collapsedSize + Padding : _collapsedSize;
                _previousPreferredSize = Target.preferredHeight;
            }
            else if(Target is HorizontalLayoutGroup)
            {
                TargetLayoutElement.preferredWidth = isExpanded ? Target.preferredWidth + _collapsedSize + Padding : _collapsedSize;
                _previousPreferredSize = Target.preferredWidth;
            }

            LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
            _isInited = true;
        }

        private float CalculateTargetPreferredSize()
        {
            if (Target is VerticalLayoutGroup)
            {
                return Target.preferredHeight + Padding;
            }
            else if (Target is HorizontalLayoutGroup)
            {
                return Target.preferredWidth + Padding;
            }

            return 0;
        }
    }
}