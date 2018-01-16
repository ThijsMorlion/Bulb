using Bulb.Controllers;

namespace Bulb.Core.Bindables
{
    public class BindLevelInt : BindableTextMeshProBase<LevelController, int>
    {
        public Level.PropertyType Type;

        protected override void Awake()
        {
            base.Awake();

            Target = ApplicationController.Instance.LevelController;
        }

        protected override int GetTargetValue()
        {
            switch (Type)
            {
                case Level.PropertyType.Battery4V:
                    return Target.CurrentLevel.Max4VBatteries;
                case Level.PropertyType.Battery9V:
                    return Target.CurrentLevel.Max9VBatteries;
                case Level.PropertyType.Bulb:
                    return Target.CurrentLevel.MaxBulbs;
                case Level.PropertyType.Buzzer:
                    return Target.CurrentLevel.MaxBuzzers;
                case Level.PropertyType.Motor:
                    return Target.CurrentLevel.MaxMotors;
                case Level.PropertyType.Switch:
                    return Target.CurrentLevel.MaxSwitches;
                case Level.PropertyType.Wire:
                    return Target.CurrentLevel.MaxWire;
                default:
                    return default(int);
            }
        }

        protected override void SetTargetValue(int value)
        {
            switch (Type)
            {
                case Level.PropertyType.Battery4V:
                    Target.CurrentLevel.Max4VBatteries = value;
                    break;
                case Level.PropertyType.Battery9V:
                    Target.CurrentLevel.Max9VBatteries = value;
                    break;
                case Level.PropertyType.Bulb:
                    Target.CurrentLevel.MaxBulbs = value;
                    break;
                case Level.PropertyType.Buzzer:
                    Target.CurrentLevel.MaxBuzzers = value;
                    break;
                case Level.PropertyType.Motor:
                    Target.CurrentLevel.MaxMotors = value;
                    break;
                case Level.PropertyType.Switch:
                    Target.CurrentLevel.MaxSwitches = value;
                    break;
                case Level.PropertyType.Wire:
                    Target.CurrentLevel.MaxWire = value;
                    break;
            }
        }

        protected override void SetUIValue(int targetValue)
        {
            if (Type != Level.PropertyType.Wire)
            {
                base.SetUIValue(targetValue);
                return;
            }

            if (targetValue != -1)
            {
                base.SetUIValue(targetValue);
                return;
            }

            var unicode = 8734;
            var character = (char)unicode;

            if (_tmProInputField != null)
            {
                base.SetUIValue(targetValue);
            }
            else if (_tmProText != null)
            {
                _tmProText.text = character.ToString();
            }
        }

        protected override int GetUIValue()
        {
            if (Type != Level.PropertyType.Wire)
                return base.GetUIValue();
            
            if (_tmProInputField != null)
            {
                return base.GetUIValue();
            }
            else if (_tmProText != null)
            {
                var text = _tmProText.text[0];
                if((decimal)text == 8734)
                    return -1;
            }

            return base.GetUIValue();
        }
    }
}
