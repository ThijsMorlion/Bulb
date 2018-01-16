using Bulb.Core;
using TMPro;

namespace Bulb.LevelEditor.Popups
{
    public class WarningPopup : BasePopup
    {
        public static WarningPopup Instance;

        public TextMeshProUGUI Header;
        public TextMeshProUGUI Text;

        public enum Type
        {
            Info,
            Warning,
            Error
        }

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }

        public void PopupMessage(Type type, string message)
        {
            Header.text = type.ToString();
            Text.text = message;
            TogglePopup(true);
        }
    }
}