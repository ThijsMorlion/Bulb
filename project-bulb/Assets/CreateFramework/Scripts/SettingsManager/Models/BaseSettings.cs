using System;
using System.IO;
using System.Xml;
using UnityEngine;
using System.Xml.Serialization;

namespace Settings.Model
{
    public class BaseSettings
    {
        [XmlIgnore, NonSerialized]
        public string CurrentSettingsVersion = "1.0.0";

        //serialized version in which the settings are created
        [XmlAttribute("SettingsVersion")]
        public string SettingsCreatedVersion;

        //default settings
        public Setting<bool> ShowStatistics = new Setting<bool> { Category = "default", DisplayName = "Show FPS counter", Description = "Show a draggable FPS counter overlay on the screen.", Defaultvalue = false };
        public Setting<bool> Fullscreen = new Setting<bool> { Category = "default", Description = "Toggle the application between fullscreen and windowed mode.", Defaultvalue = true };
        public Setting<bool> ShowCursor = new Setting<bool> { Category = "default", DisplayName = "Show cursor", Description = "Show or hide the cursor.", Defaultvalue = true };
        public Setting<bool> ShowOnTapSequence = new Setting<bool> { Category = "default", DisplayName = "Tap sequence", Description = "Choose whether the tap sequence (twice on the left bottom corner, twice on the right bottom corner) triggers this settings panel.", Defaultvalue = true };
        public Setting<string> SettingsPasscode = new Setting<string> { Category = "default", Defaultvalue = "", Description = "Numeric password asked when settingsoverlay is made visible. Password will not be asked when left empty. All non-numeric characters are stripped from password (when the password 'A0B1C2' is entered in the settingslist, the password '012' has to be entered on the passcode screen)." };
        public Setting<Vector2> ForceResolution = new Setting<Vector2> { Category = "default", DisplayName = "Force resolution", Description = "Forces the app to run in this screenresolution on startup.\nA resolution of 0x0 disables this setting.", Defaultvalue = new Vector2(0, 0) };

        public void SerializeToXmlFile()
        {
            SettingsCreatedVersion = CurrentSettingsVersion;

            XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;

            XmlSerializer serializer = new XmlSerializer(GetType());

            if (SettingsManager.AbsoluteSettingsFilePath == null)
            {
                SettingsManager.CreateXmlFilePath();
            }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            System.IO.StreamWriter file = new System.IO.StreamWriter(SettingsManager.AbsoluteSettingsFilePath);
            serializer.Serialize(file, this, emptyNamespaces);
            file.Close();
#else

            //create temp xml file
            using (XmlWriter writer = XmlWriter.Create(SettingsManager.AbsoluteTempSettingsFilePath, xmlSettings))
            {
                serializer.Serialize(writer, this, emptyNamespaces);
            }

            //overwrite old file with temp file
            if (File.Exists(SettingsManager.AbsoluteSettingsFilePath))
            {
                File.Delete(SettingsManager.AbsoluteSettingsFilePath);
            }

            File.Copy(SettingsManager.AbsoluteTempSettingsFilePath, SettingsManager.AbsoluteSettingsFilePath);
#endif
        }
    }

    public class Setting<T>
    {
        private T _value;

        public event EventHandler<SettingChangedEventArgs<T>> PropertyChanged;

        public T Value
        {
            get
            {
                if (_value == null && Defaultvalue != null)
                {
                    return Defaultvalue;
                }

                return _value;
            }
            set
            {
                T previousValue = _value;
                _value = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new SettingChangedEventArgs<T>(previousValue, value));
                }
            }
        }

        [XmlIgnore]
        public T Defaultvalue
        {
            get; set;
        }

        [XmlIgnore]
        public string Category
        {
            get; set;
        }

        [XmlIgnore]
        public string DisplayName
        {
            get;
            set;
        }

        [XmlIgnore]
        public string Description
        {
            get;
            set;
        }

        public Setting()
        {
            Value = Defaultvalue;
        }

        public Setting(T value)
        {
            Value = value;
        }
    }

    public class MinMaxSetting : Setting<float>
    {
        public float Min
        {
            get; set;
        }

        public float Max
        {
            get; set;
        }

        public MinMaxSetting()
        {
            //method required for serializing
        }

        public MinMaxSetting(float value) : base(value)
        {
            //method required for serializing
        }
    }

    public class ButtonSetting : Setting<object>
    {
        public event EventHandler OnButtonClick;

        public string ButtonContent
        {
            get; set;
        }

        public void RaiseOnButtonClickEvent()
        {
            if (OnButtonClick != null)
            {
                OnButtonClick(this, null);
            }
        }

        public ButtonSetting()
        {
            //method required for serializing
        }

        public ButtonSetting(object value) : base(value)
        {
            //method required for serializing
        }
    }

    public class SettingChangedEventArgs<T> : EventArgs
    {
        public T PreviousValue
        {
            get; private set;
        }
        public T NewValue
        {
            get; private set;
        }

        public SettingChangedEventArgs(T previousValue, T newValue)
        {
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}