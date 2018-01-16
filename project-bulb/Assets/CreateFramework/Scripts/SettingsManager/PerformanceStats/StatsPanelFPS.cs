using TMPro;
using UnityEngine;

namespace Settings.PerformanceStats
{
    public class StatsPanelFPS : MonoBehaviour
    {
        public GameObject FpsNumberTextObject;
        private TextMeshProUGUI _textMeshProUGUI;

        private void OnEnable()
        {
            _textMeshProUGUI = FpsNumberTextObject.GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _textMeshProUGUI.text = Mathf.Round((1 / Time.deltaTime)).ToString();
        }
    }
}