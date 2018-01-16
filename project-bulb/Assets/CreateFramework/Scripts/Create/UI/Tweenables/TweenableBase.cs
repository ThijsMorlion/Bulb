using UnityEngine;

namespace Create.UI.Tweenables
{
    [ExecuteInEditMode]
    public abstract class TweenableBase : MonoBehaviour
    {
        public abstract void SetInterpolation(float erpPos, bool toIn);
        public abstract void SetIn();
        public abstract void SetOut();
        public abstract void GetStartingValues(bool toIn);

        public virtual void Snap(bool toIn)
        {
            SetInterpolation(1, toIn);
        }

        public bool InStateSet { get { return _inStateSet; } }
        public bool OutStateSet { get { return _outStateSet; } }

        [SerializeField, HideInInspector]
        protected bool _inStateSet, _outStateSet;
    }
}