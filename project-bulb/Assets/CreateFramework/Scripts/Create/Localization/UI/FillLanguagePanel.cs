using UnityEngine;
using UnityEngine.UI;

namespace Create.Localization.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class FillLanguagePanel : MonoBehaviour
    {
        public GameObject LanguageTogglePrefab, SeparatorPrefab;
        private int _initialChildCount;

        void Start()
        {
            _initialChildCount = transform.childCount;
            LanguageController.PropertyChanged += LanguageController_PropertyChanged;

            if(LanguageController.Languages != null)
            {
                BuildLanguageSelectionPanel();
            }
        }

        private void LanguageController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Languages")
            {
                if (LanguageController.Languages != null)
                {
                    BuildLanguageSelectionPanel();
                }
            }
        }

        private void BuildLanguageSelectionPanel()
        {
            // Destroy all children except the ones which are there at start.
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= _initialChildCount; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            if (LanguageController.Languages == null)
                return;
            if(LanguageTogglePrefab == null)
            {
                Debug.LogWarningFormat("No language toggle prefab assigned in {0} {1}.", GetType(), gameObject.name);
                return;
            }

            for (int i = 0; i < LanguageController.Languages.Count; i++)
            {
                // Add and init language toggle.
                var languageToggle = Instantiate(LanguageTogglePrefab, transform, false);
                if(languageToggle.GetComponentInChildren<LanguageToggle>() == null)
                {
                    Debug.LogWarningFormat("Assigned language toggle prefab in {0} {1} does not have a {2} component.", GetType(), gameObject.name, typeof(LanguageToggle));
                    return;
                }
                languageToggle.GetComponentInChildren<LanguageToggle>().Language = LanguageController.Languages[i];
                languageToggle.GetComponentInChildren<Toggle>().group = GetComponent<ToggleGroup>();

                // Select the first toggle.
                if (i == 0)
                {
                    languageToggle.GetComponentInChildren<Toggle>().isOn = true;
                }

                // Add separator.
                if(SeparatorPrefab != null)
                {
                    if (i < LanguageController.Languages.Count - 1)
                    {
                        var separator = Instantiate(SeparatorPrefab);
                        separator.transform.SetParent(transform, false);
                    }
                }
            }
        }
    }
}