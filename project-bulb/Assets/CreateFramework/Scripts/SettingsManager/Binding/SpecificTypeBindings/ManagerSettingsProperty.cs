using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.ComponentModel;
using Settings.List.Description;
using System.Text.RegularExpressions;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsProperty : MonoBehaviour
    {
        public GameObject LabelTextObject;
        public GameObject DescriptionButtonObject;
        [NonSerialized]
        public SettingDescription SettingDescription;

        private FieldInfo _field;
        public FieldInfo Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                SetVectorDimensionsInChildren();
                SetToggleLabelText();
                SetPropertiesInChildren();

                OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));
            }
        }

        private void OnEnable()
        {
            SettingDescription = FindObjectOfType<SettingDescription>();
            DescriptionButtonObject.GetComponent<Button>().onClick.AddListener(() => OnDescriptionButtonClick());
        }

        private void OnDisable()
        {
            DescriptionButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        public virtual void SetVectorDimensionsInChildren()
        {
        }

        public virtual void SetToggleLabelText()
        {
            LabelTextObject.GetComponent<TextMeshProUGUI>().text = GetLabelTextFromField();
        }

        public virtual string GetLabelTextFromField()
        {
            throw new Exception("Class must override this method");
        }

        public virtual void SetPropertiesInChildren()
        {
            throw new Exception("Class must override this method");
        }

        public virtual void OnDescriptionButtonClick()
        {
            throw new Exception("Class must override this method");
        }

        public string CreateLabelTextFromFieldName()
        {
            string label = "";

            //split the fieldname on every capital letter and a space before
            foreach (var s in SplitCamelCase(Field.Name))
            {
                label += s + " ";
            }

            return label;
        }

        //split string on every capital letter
        private string[] SplitCamelCase(string source)
        {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
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
