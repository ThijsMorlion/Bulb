using Create.UI.Fonts;
using UnityEngine;

namespace Create.Localization
{
    public class SetSiblingIndexByRightToLeft : MonoBehaviour
    {
        public int LeftSiblingIndex = 0, RightSiblingIndex = 1;

        protected LanguageController _languageController;
        protected FontController _fontController;

        void OnEnable()
        {
            _languageController = GetComponentInParent<LanguageController>();
            _fontController = FindObjectOfType<FontController>();

            if (_languageController != null)
            {
                _languageController.LanguageChanged += LanguageController_LanguageChanged;
                UpdateSiblingIndex();
            }
            else
            {
                Debug.LogWarningFormat("{0} {1} must be parented to a {2}.", GetType(), gameObject.name, typeof(LanguageController));
            }
        }

        void OnDisable()
        {
            if (_languageController != null)
            {
                _languageController.LanguageChanged -= LanguageController_LanguageChanged;
            }
        }

        private void LanguageController_LanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            UpdateSiblingIndex();
        }

        private void UpdateSiblingIndex()
        {
            if (_fontController == null || _languageController.SelectedLanguage == null || _fontController.GetByCulture(_languageController.SelectedLanguage.Culture) == null)
                return;

            transform.SetSiblingIndex(_fontController.GetByCulture(_languageController.SelectedLanguage.Culture).IsRightToLeft ? RightSiblingIndex : LeftSiblingIndex);
        }
    }
}