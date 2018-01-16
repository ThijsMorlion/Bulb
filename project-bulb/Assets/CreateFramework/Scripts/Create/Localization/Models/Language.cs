using System.Runtime.Serialization;

namespace Create.Localization.Models
{
    [DataContract]
    public class Language
    {
        public const string EnglishCulture = "en-US";
        public const string ArabicCulture = "ar-SA";

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Culture { get; set; }
        /// <summary>
        /// Optional abbreviation.
        /// </summary>
        [DataMember]
        public string Abbreviation { get; set; }

        public Language() { }

        public Language(string title, string culture)
        {
            Title = title;
            Culture = culture;
        }

        public Language(string title, string culture, string abbreviation) : this(title, culture)
        {
            Abbreviation = abbreviation;
        }

        public static bool operator ==(Language one, object two)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(one, two))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)one == null) || two == null)
            {
                return false;
            }

            if (two is Language)
            {
                return one.Culture == ((Language)two).Culture;
            }

            return false;
        }

        public static bool operator !=(Language one, object two)
        {
            return !(one == two);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}