using Bulb.Controllers;

namespace Bulb.Core.Bindables
{
    public class BindLevelString : BindableTextMeshProBase<LevelController, string>
    {
        public Level.PropertyType Type;

        protected override void Awake()
        {
            base.Awake();

            Target = ApplicationController.Instance.LevelController;
        }

        protected override string GetTargetValue()
        {
            switch(Type)
            {
                case Level.PropertyType.GoalDescription:
                    return Target.CurrentLevel.GoalDescription ?? string.Empty;
                default:
                    return default(string);
            }
        }

        protected override void SetTargetValue(string value)
        {
            switch(Type)
            {
                case Level.PropertyType.GoalDescription:
                    Target.CurrentLevel.GoalDescription = value;
                    break;
            }
        }
    }
}
