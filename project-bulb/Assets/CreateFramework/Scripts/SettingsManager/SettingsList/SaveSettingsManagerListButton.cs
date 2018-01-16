using System;
using UnityEngine;
using UnityEngine.UI;

namespace Settings.List
{
    public class SaveSettingsManagerListButton : MonoBehaviour
    {
        public static event EventHandler OnSaveSettingsManagerListButtonClickEvent;

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(() => OnClick());
        }


        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }


        private void OnClick()
        {
            if (OnSaveSettingsManagerListButtonClickEvent != null)
            {
                OnSaveSettingsManagerListButtonClickEvent(this, null);
            }
        }
    }
}