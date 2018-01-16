using UnityEngine;
using System.Linq;
using TMPro;
using Create.UI.Fonts;
using System.Collections;

namespace Create.Localization.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DisplayInterfaceText : MonoBehaviour
    {
        public string Key;
        public string Prefix, Suffix;
        public FontSizes FontSize;
        /// <summary>
        /// If set, the fonts defined in the font controller aren't applied to this interface text.
        /// </summary>
        [Tooltip("If set, the fonts defined in the font controller aren't applied to this interface text.")]
        public bool OverrideFont;
        /// <summary>
        /// If set, right to left settings will not override the outlining.
        /// </summary>
        [Tooltip("If set, right to left settings will not override the outlining.")]
        public bool OverrideOutlining;

        protected TextMeshProUGUI _text;
        protected LanguageController _languageController;
        protected FontController _fontController;
        protected FontSizes _previousFontSize;
        protected IEnumerator _forceLayoutUpdateRoutine;

        protected virtual void OnEnable()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _languageController = GetComponentInParent<LanguageController>();
            _fontController = FindObjectOfType<FontController>();

            if (!Application.isPlaying)
                return;

            if (_languageController != null)
            {
                _languageController.LanguageChanged += LanguageController_LanguageChanged;
                LanguageController.PropertyChanged += LanguageController_PropertyChanged;
            }
            else
            {
                Debug.LogWarningFormat("[{0}] {1} must be parented to a {2}.", GetType(), gameObject.name, typeof(LanguageController));
            }

            _previousFontSize = FontSize;
            UpdateText();
        }

        protected virtual void OnDisable()
        {
            if (_languageController != null)
            {
                _languageController.LanguageChanged -= LanguageController_LanguageChanged;
                LanguageController.PropertyChanged -= LanguageController_PropertyChanged;
            }
        }

#if UNITY_EDITOR
        // Update font size at editor time. Use LateUpdate to avoid conflicts with child classes.
        void LateUpdate()
        {
            if (Application.isPlaying)
                return;

            if(_previousFontSize != FontSize)
            {
                _previousFontSize = FontSize;
                ApplyFontSettings();
            }
        }
#endif

        public void ChangeKey(string key)
        {
            Key = key;
            UpdateText();
        }

        protected virtual void LanguageController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Update text when the interface texts are updated.
            if (e.PropertyName == "InterfaceTexts")
            {
                UpdateText();
            }
        }

        protected virtual void LanguageController_LanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            // Update text when the selected language changes.
            UpdateText();
        }

        protected virtual void UpdateText()
        {
            // If no key is assigned or the interface texts are not yet loaded, assign null.
            if (string.IsNullOrEmpty(Key))
            {
                _text.text = "No key assigned";
                return;
            }
            if (LanguageController.InterfaceTexts == null)
            {
                _text.text = "Interface texts are not yet loaded.";
                return;
            }
            if (_languageController == null)
            {
                _text.text = "Language controller is null.";
                return;
            }
            if (_languageController.SelectedLanguage == null || _languageController.SelectedLanguage.Culture == null)
            {
                _text.text = "Selected language or culture is null.";
                // Attempt to select default language.
                _languageController.SelectDefaultLanguage();
                return;
            }

            // Get and apply the required interface text.
            string text = GetLocalizedText();
            if (text != null)
            {
                ApplyFontSettings();

                _text.text = string.Format("{0}{1}{2}", Prefix, text, Suffix);
                ForceLayoutUpdate();
            }
            // If the key doesn't exist, show that it is missing.
            else
            {
                ApplyFontSettings(FontController.Default);
                _text.text = string.Format("? {0}", Key);
                Debug.LogWarningFormat("[{0}] No interface text found for key \"{1}\".", GetType(), Key);
            }
        }

        protected virtual string GetLocalizedText()
        {
            var interfaceText = LanguageController.InterfaceTexts.Where(i => i.Key == Key).FirstOrDefault();
            if (interfaceText == null)
                return null;

            // Select the desired text, or default culture if the translation is not filled in.
            string text = interfaceText[_languageController.SelectedLanguage.Culture];
            if (string.IsNullOrEmpty(text))
            {
                text = interfaceText[LanguageController.DefaultCulture];
                ApplyFontSettings(FontController.Default);
            }

            return text;
        }

        private void ForceLayoutUpdate()
        {
            if(_forceLayoutUpdateRoutine != null)
            {
                StopCoroutine(_forceLayoutUpdateRoutine);
            }

            _text.SetLayoutDirty();
            _forceLayoutUpdateRoutine = RunForceLayoutUpdate();
            StartCoroutine(_forceLayoutUpdateRoutine);
        }

        private IEnumerator RunForceLayoutUpdate()
        {
            yield return null;
            _text.text = _text.text;
            _text.SetLayoutDirty();
            yield return null;
            _text.text = _text.text;

            _forceLayoutUpdateRoutine = null;
        }

        protected virtual void ApplyFontSettings(string culture = null)
        {
            if (_fontController == null)
                return;

            string selectedCulture = FontController.Default;
            if (_languageController != null && _languageController.SelectedLanguage != null)
            {
                selectedCulture = _languageController.SelectedLanguage.Culture;
            }

            var fontInfo = _fontController.GetByCulture(culture != null ? culture : selectedCulture);
            if (fontInfo == null)
                return;

            // Apply font size.
            switch(FontSize)
            {
                case FontSizes.Header1:
                    _text.fontSize = fontInfo.H1Size;
                    break;
                case FontSizes.Header2:
                    _text.fontSize = fontInfo.H2Size;
                    break;
                case FontSizes.Header3:
                    _text.fontSize = fontInfo.H3Size;
                    break;
                case FontSizes.Body:
                    _text.fontSize = fontInfo.BodySize;
                    break;
                case FontSizes.Small:
                    _text.fontSize = fontInfo.SmallSize;
                    break;
                case FontSizes.Button:
                    _text.fontSize = fontInfo.ButtonSize;
                    break;
            }

            // Apply font.
            if(!OverrideFont)
            {
                _text.font = fontInfo.Font;
            }

            // Apply right to left.
            _text.isRightToLeftText = fontInfo.IsRightToLeft;
            if(!OverrideOutlining)
            {
                if(_text.alignment == TextAlignmentOptions.Left || _text.alignment == TextAlignmentOptions.Right)
                {
                    _text.alignment = _text.isRightToLeftText ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
                }
                else if(_text.alignment == TextAlignmentOptions.TopLeft || _text.alignment == TextAlignmentOptions.TopRight)
                {
                    _text.alignment = _text.isRightToLeftText ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
                }
                else if(_text.alignment == TextAlignmentOptions.BottomLeft || _text.alignment == TextAlignmentOptions.BottomRight)
                {
                    _text.alignment = _text.isRightToLeftText ? TextAlignmentOptions.BottomRight : TextAlignmentOptions.BottomLeft;
                }
            }
        }
    }
}