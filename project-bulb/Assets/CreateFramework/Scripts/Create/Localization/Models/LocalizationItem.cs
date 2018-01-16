using System.Collections.Generic;
using ArabicSupport;
using System.Xml.Serialization;

namespace Create.Localization.Models
{
    public class LocalizationItem
    {
        public string Key { get; set; }

        private List<LocalizationKeyValue> _localizationData = new List<LocalizationKeyValue>();
        /// <summary>
        /// Used for serialization only - use the [] operator to get the interface text value for a given Language or language code.
        /// </summary>
        [XmlArray]
        [XmlArrayItem(ElementName = "Localization")]
        public List<LocalizationKeyValue> LocalizationData
        {
            get { return _localizationData; }
            set { _localizationData = value; }
        }

        public string this[string languageCode]
        {
            get
            {
                return GetValueByCulture(languageCode);
            }
        }

        public string this[Language language]
        {
            get
            {
                if (language == null || language.Culture == null)
                    return null;
                return GetValueByCulture(language.Culture);
            }
        }

        protected Dictionary<string, string> _internalDictionary;

        public LocalizationItem() { }

        public LocalizationItem(string culture, string value)
        {
            AddOrUpdateLanguage(culture, value);
        }

        public LocalizationItem(string key, string culture, string value) : this(culture, value)
        {
            Key = key;
        }

        public virtual LocalizationItem AddOrUpdateLanguage(string culture, string value)
        {
            // Fix Arabic texts when added - fixing them at runtime creates a performance spike due to massive garbage creation.
            if (culture == Language.ArabicCulture)
            {
                value = ArabicFixer.Fix(value);
            }

            if (_internalDictionary == null)
                _internalDictionary = new Dictionary<string, string>();

            if (_internalDictionary.ContainsKey(culture))
            {
                _internalDictionary[culture] = value;
            }
            else
            {
                _internalDictionary.Add(culture, value);
            }

            return this;
        }

        public bool ShouldSerializeKey()
        {
            return !string.IsNullOrEmpty(Key);
        }

        protected virtual string GetValueByCulture(string culture)
        {
            if (_internalDictionary == null)
                ConstructInternalDictionary();
            if (_internalDictionary == null)
                return null;

            if (!_internalDictionary.ContainsKey(culture))
            {
                if (!string.IsNullOrEmpty(Key))
                {
                    return string.Format("{0} is not included in {1}.", culture, Key);
                }
                else
                {
                    return null;
                }
            }

            return _internalDictionary[culture];
        }

        protected virtual void ConstructInternalDictionary()
        {
            if (LocalizationData == null)
                return;

            foreach (var item in LocalizationData)
            {
                AddOrUpdateLanguage(item.Culture, item.Text);
            }
        }
    }

    public class LocalizationKeyValue
    {
        [XmlAttribute("language")]
        public string Culture { get; set; }
        public string Text { get; set; }

        public LocalizationKeyValue() { }

        public LocalizationKeyValue(string culture, string value)
        {
            Culture = culture;
            Text = value;
        }
    }
}