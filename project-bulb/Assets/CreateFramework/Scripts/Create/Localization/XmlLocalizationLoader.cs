using Create.Helpers;
using Create.Localization.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Create.Localization
{
    public class XmlLocalizationLoader : BaseLocalizationLoader
    {
        public string RelativeLanguagesPath = "languages.xml";
        public string RelativeInterfaceTextsPath = "localization.xml";

        public void CreateDummyXmls()
        {
            if (!string.IsNullOrEmpty(RelativeLanguagesPath))
            {
                try
                {
                    List<Language> languages = new List<Language>() { new Language() { Culture = "en-US", Title = "English" } };
                    using (var stream = new StreamWriter(File.Open(Path.Combine(Application.streamingAssetsPath, RelativeLanguagesPath), FileMode.CreateNew), Encoding.UTF8))
                    {
                        var serializer = new XmlSerializer(typeof(List<Language>));
                        serializer.Serialize(stream, languages);
                        Debug.LogFormat("[{0}] Dummy language xml created.", GetType());
                    }
                }
                catch (Exception) { }
            }

            if (!string.IsNullOrEmpty(RelativeInterfaceTextsPath))
            {
                try
                {
                    List<LocalizationItem> interfaceTexts = new List<LocalizationItem>()
                    {
                        new LocalizationItem()
                        {
                            Key = "Key.ItemOne",
                            LocalizationData = new List<LocalizationKeyValue>()
                            {
                                new LocalizationKeyValue("en-US", "English text"),
                                new LocalizationKeyValue("nl-BE", "Nederlandse tekst")
                            }
                        },
                        new LocalizationItem()
                        {
                            Key = "Key.ItemTwo",
                            LocalizationData = new List<LocalizationKeyValue>()
                            {
                                new LocalizationKeyValue("en-US", "English text"),
                                new LocalizationKeyValue("nl-BE", "Nederlandse tekst")
                            }
                        },
                    };

                    using (var stream = new StreamWriter(File.Open(Path.Combine(Application.streamingAssetsPath, RelativeInterfaceTextsPath), FileMode.CreateNew), Encoding.UTF8))
                    {
                        var serializer = new XmlSerializer(typeof(List<LocalizationItem>));
                        serializer.Serialize(stream, interfaceTexts);
                        Debug.LogFormat("[{0}] Dummy interface texts xml created.", GetType());
                    }
                }
                catch (Exception) { }
            }
        }

        protected override IEnumerator LoadLanguages()
        {
            var parseLanguages = new AsyncXmlParser<List<Language>>(Path.Combine(Application.streamingAssetsPath, RelativeLanguagesPath));
            yield return parseLanguages;
            LanguageController.Languages = parseLanguages.Result;
        }

        protected override IEnumerator LoadLocalizationItems()
        {
            var parseInterfaceTexts = new AsyncXmlParser<List<LocalizationItem>>(Path.Combine(Application.streamingAssetsPath, RelativeInterfaceTextsPath));
            yield return parseInterfaceTexts;
            LanguageController.InterfaceTexts = parseInterfaceTexts.Result;
        }
    }
}