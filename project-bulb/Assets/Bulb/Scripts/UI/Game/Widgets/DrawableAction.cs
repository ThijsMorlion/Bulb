using UnityEngine;
using UnityEngine.UI;

namespace Bulb.UI.Game
{
    [RequireComponent(typeof(Button))]
    public class DrawableAction : MonoBehaviour
    {
        public enum ActionType
        {
            RotateLeft,
            RotateRight,
            Delete,
            None
        }

        public ActionType Type = ActionType.None;
        public Button.ButtonClickedEvent OnClick
        {
            get
            {
                return GetComponent<Button>().onClick;
            }
        }

        public void Start()
        {
            var actionWidget = ActionWidget.Instance;
            actionWidget.RegisterAction(this);
        }
    }
}
