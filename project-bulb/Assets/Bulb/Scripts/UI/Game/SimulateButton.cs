using Bulb.Controllers;
using Bulb.Electricity;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.UI.Game
{
    [RequireComponent(typeof(Button))]
    public class SimulateButton : MonoBehaviour
    {
        public Sprite PlayImage;
        public Sprite StopImage;
        public Image Icon;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ToggleSimulation);

            var currentWalker = ApplicationController.Instance.CurrentWalker;
            currentWalker.SetIsSimulating(false);

            CurrentWalker.OnSimulationFailed += CurrentWalker_OnSimulationFailed;
        }

        private void OnDisable()
        {
            CurrentWalker.OnSimulationFailed -= CurrentWalker_OnSimulationFailed;
        }

        private void CurrentWalker_OnSimulationFailed(ErrorCode code)
        {
            var currentWalker = ApplicationController.Instance.CurrentWalker;
            currentWalker.SetIsSimulating(false);
            Icon.sprite = PlayImage;
        }

        private void ToggleSimulation()
        {
            var currentWalker = ApplicationController.Instance.CurrentWalker;
            if(CurrentWalker.IsSimulating)
            {
                currentWalker.SetIsSimulating(false);
                Icon.sprite = PlayImage;
            }
            else
            {
                currentWalker.SetIsSimulating(true);
                Icon.sprite = StopImage;

                currentWalker.AnalyzeCircuit();
            }
        }
    }
}
