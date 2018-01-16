using Create.Localization.Models;
using Create.UI.Resetable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Create.Localization
{
    public class LanguageController : ResetableBase
    {
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        public static event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        public const string DefaultCulture = Language.EnglishCulture;

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (_selectedLanguage == value)
                    return;

                _selectedLanguage = value;
                if (LanguageChanged != null)
                    LanguageChanged(null, new LanguageChangedEventArgs(value));
            }
        }

        private static List<Language> _languages;
        /// <summary>
        /// The available languages.
        /// </summary>
        public static List<Language> Languages
        {
            get { return _languages; }
            set
            {
                if (_languages == value)
                    return;

                _languages = value;
                RaisePropertyChanged("Languages");
            }
        }

        private static List<LocalizationItem> _interfaceTexts;
        /// <summary>
        /// All interface texts.
        /// </summary>
        public static List<LocalizationItem> InterfaceTexts
        {
            get { return _interfaceTexts; }
            set
            {
                if (_interfaceTexts == value)
                    return;

                _interfaceTexts = value;
                RaisePropertyChanged("InterfaceTexts");
            }
        }

        void OnEnable()
        {
            PropertyChanged += LanguageController_PropertyChanged;
        }

        void OnDisable()
        {
            PropertyChanged -= LanguageController_PropertyChanged;
        }

        public virtual void SelectDefaultLanguage()
        {
            if (Languages == null)
            {
                Debug.LogWarning("Languages are not yet loaded - can't select default language.");
                return;
            }

            // Default language is already selected.
            if (SelectedLanguage != null && SelectedLanguage.Culture == DefaultCulture)
                return;

            SelectedLanguage = Languages.Where(l => l.Culture == DefaultCulture).FirstOrDefault();
            if (SelectedLanguage == null)
            {
                Debug.LogWarningFormat("Target default language {0} is not in the list of available languages.", DefaultCulture);
            }
        }

        public override void ResetUIElements()
        {
            SelectDefaultLanguage();
        }

        protected static void RaisePropertyChanged(string propname)
        {
            if (PropertyChanged != null)
                PropertyChanged(null, new PropertyChangedEventArgs(propname));
        }

        private void LanguageController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Languages")
            {
                SelectDefaultLanguage();
            }
        }
    }

    public class LanguageChangedEventArgs : EventArgs
    {
        public Language Language { get; private set; }
        public LanguageChangedEventArgs(Language language)
        {
            Language = language;
        }
    }
}