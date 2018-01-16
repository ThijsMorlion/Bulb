using Create.Helpers;
using Create.Localization.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Create.Localization
{
    public class CsvLocalizationLoader : BaseLocalizationLoader
    {
        public string CsvStreamingAssetsPath = "Localization.txt";

        private List<string[]> _csvData;

        protected override IEnumerator LoadLanguages()
        {
            // Load the csv containing the localized data.
            var csvLoader = new AsyncCsvLoader(Path.Combine(Application.streamingAssetsPath, CsvStreamingAssetsPath), '\t');
            yield return csvLoader;
            _csvData = csvLoader.Result;
            if (_csvData == null)
                yield break;
            if(_csvData.Count == 0 || _csvData[0].Length < 2)
            {
                Debug.LogWarningFormat("[{0}] Invalid localization csv {1}. Make sure the csv is tab delimited.", GetType(), CsvStreamingAssetsPath);
                yield break;
            }


            var languages = new List<Language>();
            // Use the headers line as source for languages.
            for(int i = 1; i < _csvData[0].Length; i++)
            {
                string[] parts = _csvData[0][i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length != 2)
                {
                    Debug.LogWarningFormat("[{0}] Invalid location csv {1}: Language headers are expected to be in format [culture]|[Name], i.e. en-US|English.", GetType(), CsvStreamingAssetsPath);
                    yield break;
                }

                languages.Add(new Language(parts[1], parts[0]));
            }

            LanguageController.Languages = languages;
        }

        protected override IEnumerator LoadLocalizationItems()
        {
            if (LanguageController.Languages == null || _csvData.Count < 2)
                yield break;

            var intTexts = new List<LocalizationItem>();
            for(int i = 1; i < _csvData.Count; i++)
            {
                intTexts.Add(new LocalizationItem(_csvData[i][0], LanguageController.Languages[0].Culture, _csvData[i][1]));
                if(_csvData[i].Length > 2)
                {
                    for(int j = 2; j < _csvData[i].Length; j++)
                    {
                        intTexts[intTexts.Count - 1].AddOrUpdateLanguage(LanguageController.Languages[j - 1].Culture, _csvData[i][j]);
                    }
                }
            }

            LanguageController.InterfaceTexts = intTexts;

            // Clear the cached csv data.
            _csvData = null;
        }
    }
}