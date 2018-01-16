using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Create.UI.Fonts
{
    [ExecuteInEditMode]
    public class FontController : MonoBehaviour
    {
        public const string Default = "Default";

        [SerializeField, HideInInspector]
        protected List<FontInfo> _fontSettings;

        void Awake()
        {
            if (_fontSettings == null)
            {
                InitDefaultFont();
            }
        }

        public void AddFontSetting()
        {
            if (_fontSettings == null)
                return;

            _fontSettings.Add(new FontInfo());
        }

        public void RemoveFontSettingAtIndex(int index)
        {
            if (_fontSettings == null)
                return;
            if (index < 0 || index >= _fontSettings.Count)
                return;

            _fontSettings.RemoveAt(index);
        }

        /// <summary>
        /// Returns either the target font info, or the default if no special case is present for the given culture.
        /// </summary>
        public FontInfo GetByCulture(string culture)
        {
            if (_fontSettings == null || _fontSettings.Count == 0)
                return null;

            var fontInfo = _fontSettings.Where(f => f.Culture == culture).FirstOrDefault();
            if (fontInfo != null)
            {
                return fontInfo;
            }

            return _fontSettings[0];
        }

        public FontInfo GetByIndex(int index)
        {
            if (_fontSettings == null || index < 0 || index >= _fontSettings.Count)
                return null;

            return _fontSettings[index];
        }

        public int Count()
        {
            if (_fontSettings == null)
                return 0;
            return _fontSettings.Count;
        }

        private void InitDefaultFont()
        {
            _fontSettings = new List<FontInfo>()
            {
                new FontInfo()
                {
                    Culture = Default
                }
            };
        }
    }

    [Serializable]
    public class FontInfo
    {
        public string Culture;
        public TMPro.TMP_FontAsset Font;
        public float SmallSize;
        public float ButtonSize;
        public float BodySize;
        public float H1Size;
        public float H2Size;
        public float H3Size;
        public bool IsRightToLeft;

        public FontInfo()
        {
            SmallSize = 10;
            ButtonSize = BodySize = 15;
            H1Size = 25;
            H2Size = 20;
            H3Size = 18;
        }

        public void CopyFontSizes(FontInfo other)
        {
            if (other == null)
                return;

            SmallSize = other.SmallSize;
            ButtonSize = other.ButtonSize;
            BodySize = other.BodySize;
            H1Size = other.H1Size;
            H2Size = other.H2Size;
            H3Size = other.H3Size;
        }

        public float GetFontSize(FontSizes size)
        {
            switch (size)
            {
                case FontSizes.Header1:
                    return H1Size;
                case FontSizes.Header2:
                    return H2Size;
                case FontSizes.Header3:
                    return H3Size;
                case FontSizes.Body:
                    return BodySize;
                case FontSizes.Small:
                    return SmallSize;
                case FontSizes.Button:
                    return ButtonSize;
                default:
                    return 0;
            }
        }
    }

    public enum FontSizes
    {
        Custom,
        Header1,
        Header2,
        Header3,
        Body,
        Small,
        Button
    }
}