using Bulb.Controllers;
using Create.Binding;

namespace Bulb.Core.Bindables
{
    public class BindLevelBool : BindableBase<LevelController, bool>
    {
        public Level.PropertyType Type;

        protected override void Awake()
        {
            base.Awake();

            Target = ApplicationController.Instance.LevelController;
        }

        protected override bool GetTargetValue()
        {
            switch(Type)
            {
                case Level.PropertyType.CanBridge:
                    return Target.CurrentLevel.CanBridge;
                case Level.PropertyType.CanSnap:
                    return Target.CurrentLevel.CanSnap;
                case Level.PropertyType.CanBranch:
                    return Target.CurrentLevel.CanBranch;
                case Level.PropertyType.SuccessByShortCircuit:
                    return Target.CurrentLevel.SuccessByShortCircuit;
                default:
                    return false;
            }
        }

        protected override void SetTargetValue(bool value)
        {
            switch (Type)
            {
                case Level.PropertyType.CanBridge:
                    Target.CurrentLevel.CanBridge = value;
                    break;
                case Level.PropertyType.CanSnap:
                    Target.CurrentLevel.CanSnap = value;
                    break;
                case Level.PropertyType.CanBranch:
                    Target.CurrentLevel.CanBranch = value;
                    break;
                case Level.PropertyType.SuccessByShortCircuit:
                    Target.CurrentLevel.SuccessByShortCircuit = value;
                    break;
            }
        }
    }
}
