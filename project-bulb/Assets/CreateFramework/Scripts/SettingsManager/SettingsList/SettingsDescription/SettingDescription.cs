using TMPro;
using UnityEngine;
using System.Reflection;
using System.ComponentModel;

namespace Settings.List.Description
{
    public class SettingDescription : MonoBehaviour
    {
        public GameObject TitleTextObject;
        public GameObject DescriptionTextObject;

        private string _titleText;
        public string TitleText
        {
            get
            {
                return _titleText;
            }
            set
            {
                _titleText = value;
                OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));
                SetTitleText();
            }
        }

        private string _descriptionText;
        public string DescriptionText
        {
            get
            {
                return _descriptionText;
            }
            set
            {
                _descriptionText = value;
                if (_descriptionText == null || _descriptionText == "")
                {
                    _descriptionText = "<i>No description for this setting.</i>";
                }

                SetDescriptionText();
            }
        }

        private void SetTitleText()
        {
            TitleTextObject.GetComponent<TextMeshProUGUI>().text = _titleText;
        }

        private void SetDescriptionText()
        {
            DescriptionTextObject.GetComponent<TextMeshProUGUI>().text = _descriptionText;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}