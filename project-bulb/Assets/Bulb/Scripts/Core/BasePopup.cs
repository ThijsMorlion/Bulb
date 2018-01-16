using UnityEngine;
using Create.UI.Tweenables;
using UnityEngine.Events;

namespace Bulb.Core
{
    [RequireComponent(typeof(TweenableGroup))]
    public class BasePopup : MonoBehaviour
    {
        public delegate void PopupClosed();
        public event PopupClosed OnPopupClosed;

        public OnPopupCanceledEvent OnPopupCanceled;

        public virtual void TogglePopup(bool show)
        {
            GetComponent<TweenableGroup>().TweenToState(show);

            if(!show)
            {
                if (OnPopupClosed != null)
                    OnPopupClosed();
            }
        }

        public virtual void SetScreenPosition(Vector2 pos)
        {
            transform.position = pos;
        }

        public virtual void Awake()
        {
            OnPopupCanceled = new OnPopupCanceledEvent();
        }

        public virtual void Start()
        {

        }

        public void CancelPopup()
        {
            TogglePopup(false);

            if (OnPopupCanceled != null)
            {
                OnPopupCanceled.Invoke();
            }
        }
    }

    public class OnPopupCanceledEvent : UnityEvent
    {

    }
}