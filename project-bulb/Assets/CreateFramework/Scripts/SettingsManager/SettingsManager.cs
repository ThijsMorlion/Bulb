using System;
using System.IO;
using System.Xml;
using UnityEngine;
using Settings.List;
using Settings.Model;
using System.Reflection;
using System.Collections;
using Settings.Pagination;
using System.ComponentModel;
using System.Xml.Serialization;

/*
    Steps to add new setting type:
    1. Create BindManagerSettings and ManagerSettingsProperty scripts in '/SettinsManager/Binding/SpecificTypeBinding'
    2. Update all type specific logic in the above scripts to the new type
    3. Create new SettingsManagerPrefab
    4. Let the prefab be instantiated in '/SettingsManager/SettingsList/SettingsManagerList.cs'
    5. When a non-generic type is added: update 'CompareReadSettingsWithModelAndFillValues()'
*/

public class SettingsManager : MonoBehaviour
{
    public static string SettingsFolderPath = "XML/Settings/";
    public static string SettingsFilePath = "SavedSettings.xml";
    public static string AbsoluteSettingsFilePath;
    public static string AbsoluteTempSettingsFilePath;

    private static SettingsManager _instance;
    private static ExtendedManagerSettings _settingsReadFromXml;
    private SettingsManagerVisibility _settingsManagerVisibility;

    private static ExtendedManagerSettings _settings = new ExtendedManagerSettings();
    public static ExtendedManagerSettings Settings
    {
        get
        {
            return _settings;
        }
        set
        {
            _settings = value;
            OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));

            if (Settings != null)
            {
                ListenToBaseSettingsEvents();
                ApplyBaseSettings();
            }
        }
    }

    private void OnEnable()
    {
        _instance = this;
        SaveSettingsManagerListButton.OnSaveSettingsManagerListButtonClickEvent += SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent;

        _settingsManagerVisibility = GetComponent<SettingsManagerVisibility>();
        _settingsManagerVisibility.PropertyChanged += SettingsManagerVisibility_PropertyChanged;

        CreateXmlFilePath();
        StartCoroutine(RunCopySettingsToPersistentFolder());
    }

    private void OnDisable()
    {
        SaveSettingsManagerListButton.OnSaveSettingsManagerListButtonClickEvent -= SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent;
        _settingsManagerVisibility.PropertyChanged -= SettingsManagerVisibility_PropertyChanged;
    }

    private void SettingsManagerVisibility_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsSettingsManagerVisible")
        {
            if (_settingsManagerVisibility.IsSettingsManagerVisible)
            {
                SetCursorVisiblity(true);
            }
            else
            {
                SetCursorVisiblity(Settings.ShowCursor.Value);
            }
        }
    }

    private static void Fullscreen_PropertyChanged(object sender, SettingChangedEventArgs<bool> e)
    {
        SetFullscreenMode(e.NewValue);
    }

    private static void ShowCursor_PropertyChanged(object sender, SettingChangedEventArgs<bool> e)
    {
        SettingsManagerVisibility settingsManagerVisibility = FindObjectOfType<SettingsManagerVisibility>();
        if (!settingsManagerVisibility.IsSettingsManagerVisible)
        {
            SetCursorVisiblity(e.NewValue);
        }
    }

    private void SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent(object sender, EventArgs e)
    {
        _settings.SerializeToXmlFile();
    }

    private static void ListenToBaseSettingsEvents()
    {
        Settings.ShowCursor.PropertyChanged += ShowCursor_PropertyChanged;
        Settings.Fullscreen.PropertyChanged += Fullscreen_PropertyChanged;
    }

    private static void ApplyBaseSettings()
    {
        SetCursorVisiblity(Settings.ShowCursor.Value);
        SetFullscreenMode(Settings.Fullscreen.Value);
        SetScreenResolution(Settings.ForceResolution.Value);
    }

    public static void CreateXmlFilePath()
    {
#if UNITY_EDITOR
        AbsoluteSettingsFilePath = Path.Combine(Path.Combine(Application.streamingAssetsPath, SettingsFolderPath), SettingsFilePath);
#else
        AbsoluteSettingsFilePath = Path.Combine(Path.Combine(Application.persistentDataPath, SettingsFolderPath), SettingsFilePath);
#endif
        AbsoluteTempSettingsFilePath = AbsoluteSettingsFilePath + ".tmp";

        if (!File.Exists(AbsoluteSettingsFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteSettingsFilePath));
        }
    }

    private IEnumerator RunCopySettingsToPersistentFolder()
    {
        //skip this if not using persistent data path: e.g. Unity Editor
#if UNITY_EDITOR
        ReadXmlSettings();
        yield break;
#endif

#pragma warning disable CS0162 // Unreachable code detected
        if (File.Exists(AbsoluteSettingsFilePath))
#pragma warning restore CS0162 // Unreachable code detected
        {
            ReadXmlSettings();
            yield break;
        }

        var streaming = Application.streamingAssetsPath + "/";
#if UNITY_IOS && !UNITY_EDITOR
		streaming = Application.dataPath + "/Raw/";
#elif UNITY_ANDROID && !UNITY_EDITOR
		streaming = Application.dataPath + "!/assets/";
#endif

        var fileprefix = "file://";
#if UNITY_ANDROID && !UNITY_EDITOR
		fileprefix = "jar:file://";
#endif

        Debug.LogWarning("No settings in PersistentDataPath: Copying settings from StreamingAssets.");

        //WWW www = new WWW (fileprefix + streaming + SettingsFolderPath + SettingsFilePath);
        IEnumerator www = Create.Helpers.WebLoader.LoadTextFile(fileprefix + streaming + SettingsFolderPath + SettingsFilePath);
        yield return www;

        string text = www.Current as string;
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError(string.Format("[{0}] Copying SavedSettings failed.", this.ToString()));
            yield break;
        }

        File.WriteAllText(AbsoluteSettingsFilePath, text);
        yield return null;

        ReadXmlSettings();
    }

    private void ReadXmlSettings()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ExtendedManagerSettings));
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        System.IO.StreamReader file = new System.IO.StreamReader(AbsoluteSettingsFilePath);
        _settingsReadFromXml = (ExtendedManagerSettings)serializer.Deserialize(file);
        file.Close();
#else
        if (File.Exists(AbsoluteSettingsFilePath)) //only compare if xml file exist
        {
            using (FileStream fileStream = new FileStream(AbsoluteSettingsFilePath, FileMode.Open))
            {
                _settingsReadFromXml = (ExtendedManagerSettings)serializer.Deserialize(fileStream);
            }
        }
        else if (File.Exists(AbsoluteTempSettingsFilePath))
        {
            using (FileStream fileStream = new FileStream(AbsoluteTempSettingsFilePath, FileMode.Open))
            {
                _settingsReadFromXml = (ExtendedManagerSettings)serializer.Deserialize(fileStream);
            }
        }
        else
        {
            Debug.LogError("AbsoluteSettingsFilePath and AbsoluteSettingsFilePath empty.");
            _settingsReadFromXml = new ExtendedManagerSettings();
        }
#endif
        CompareReadSettingsWithModelAndFillValues();
    }

    private void CompareReadSettingsWithModelAndFillValues()
    {
        //compare the settings read from the xml with the settings read from the model
        //copy everything from the model, but keep the value from the xml
        //this way editing the model (Displayname, Defaultvalue...) is applied, but the saved value from the settings in the xml remains the same

        ExtendedManagerSettings mergedSettings = new ExtendedManagerSettings();

        var modelFields = _settings.GetType().GetFields();
        foreach (FieldInfo modelField in modelFields)
        {
            //if the setting exists in the the xml file, overwrite the default value with the read value
            foreach (FieldInfo readField in _settingsReadFromXml.GetType().GetFields())
            {
                //if the same fields have been found (one field read from the xml, the other coming from the model) 
                if (modelField.Name == readField.Name)
                {
                    if (modelField.Name == "SettingsCreatedVersion" || modelField.Name == "CurrentSettingsVersion") //settingsversion is a property but not a setting
                    {
                        if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
                        {
                            string modelVersion = ((string)modelField.GetValue(_settings));
                            string readSettingsVersion = ((string)readField.GetValue(_settingsReadFromXml));

                            if (modelVersion != readSettingsVersion)
                            {
                                Debug.LogWarningFormat("Settings created with other version of the settingsmanager. It's advised that you remove the old SettingsCanvasPrefab from your scene and replace it with the new one. \nVersion went from '{0}' to '{1}' \n----------------------------", readSettingsVersion, modelVersion);
                            }
                        }

                        continue; //skip to other iteration
                    }

                    //copy everything from the model, but keep the value from the xml
                    if (modelField.FieldType.IsGenericType) //generic type: string, bool, vector2...
                    {
#if !UNITY_IOS
                        if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
                        {
                            //setting is present in xml, apply the xml value to the field
                            var type = modelField.FieldType.GetGenericArguments()[0];
                            var genericMergeSettingsMethod = GetType().GetMethod("MergeSettings", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type);
                            genericMergeSettingsMethod.Invoke(this, new object[] { mergedSettings, modelField, readField });
                        }
                        else
                        {
                            //setting is not present in xml, apply the default value to the field
                            var type = modelField.FieldType.GetGenericArguments()[0];
                            var genericApplyDefaultSettingsMethod = GetType().GetMethod("ApplyDefaultValueOnSetting", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type);
                            genericApplyDefaultSettingsMethod.Invoke(this, new object[] { mergedSettings, modelField });
                        }
#else

						if (modelField.FieldType.GetGenericArguments()[0] == typeof(bool))
						{
							if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
								MergeSettings_bool(mergedSettings, modelField, readField );
							else
								ApplyDefaultValueOnSetting_bool(mergedSettings, modelField );
							
						}else if (modelField.FieldType.GetGenericArguments()[0] == typeof(string))
						{
							if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
								MergeSettings_string(mergedSettings, modelField, readField);
							else
								ApplyDefaultValueOnSetting_string(mergedSettings, modelField);

						}else if (modelField.FieldType.GetGenericArguments()[0] == typeof(float))
						{
							if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
								MergeSettings_float(mergedSettings, modelField, readField);
							else
								ApplyDefaultValueOnSetting_float(mergedSettings, modelField);

						}else if (modelField.FieldType.GetGenericArguments()[0] == typeof(Vector2))
						{
							if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
								MergeSettings_vector2(mergedSettings, modelField, readField);
							else
								ApplyDefaultValueOnSetting_vector2(mergedSettings, modelField);

						}else if (modelField.FieldType.GetGenericArguments()[0] == typeof(Vector3))
						{
							if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
								MergeSettings_vector3(mergedSettings, modelField, readField);
							else
								ApplyDefaultValueOnSetting_vector3(mergedSettings, modelField);

						} 
						else
						{
							Debug.LogWarning(string.Format("Field of type '{0}' not yet implemented.", modelField.FieldType.Name));
						}
#endif
                    }
                    else //non generic type
                    {
                        if (IsSettingAlreadyPresentInXmlFile(modelField.Name))
                        {
                            //setting is present in xml, apply the xml value to the field
                            if (modelField.FieldType == typeof(MinMaxSetting))
                            {
                                MinMaxSetting mergedSetting = ((MinMaxSetting)modelField.GetValue(_settings));
                                MinMaxSetting readSetting = ((MinMaxSetting)readField.GetValue(_settingsReadFromXml));
                                mergedSetting.Value = readSetting.Value;
                                mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
                            }
                            else if (modelField.FieldType == typeof(ButtonSetting))
                            {
                                ButtonSetting mergedSetting = ((ButtonSetting)modelField.GetValue(_settings));
                                ButtonSetting readSetting = ((ButtonSetting)readField.GetValue(_settingsReadFromXml));
                                mergedSetting.Value = readSetting.Value;
                                mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("Field of type '{0}' not yet implemented.", modelField.FieldType.Name));
                            }
                        }
                        else
                        {
                            if (modelField.FieldType == typeof(MinMaxSetting))
                            {
                                //setting is not present in xml, apply the default value to the field
                                MinMaxSetting mergedSetting = ((MinMaxSetting)modelField.GetValue(_settings));
                                mergedSetting.Value = mergedSetting.Defaultvalue;
                                mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
                            }
                            else if (modelField.FieldType == typeof(ButtonSetting))
                            {
                                //setting is not present in xml, apply the default value to the field
                                ButtonSetting mergedSetting = ((ButtonSetting)modelField.GetValue(_settings));
                                mergedSetting.Value = mergedSetting.Defaultvalue;
                                mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("Field of type '{0}' not yet implemented.", modelField.FieldType.Name));
                            }
                        }
                    }
                }
            }
        }

        Settings = mergedSettings;

        //save the merged settings to the xml file
        Settings.SerializeToXmlFile();
    }

    private bool IsSettingAlreadyPresentInXmlFile(string settingName)
    {
        XmlDocument doc = new XmlDocument();
        if (File.Exists(AbsoluteSettingsFilePath))
        {
            doc.Load(AbsoluteSettingsFilePath);
        }
        else if (File.Exists(AbsoluteTempSettingsFilePath))
        {
            doc.Load(AbsoluteTempSettingsFilePath);
        }
        else
        {
            //xml files do not exist
            Debug.LogWarning("Settings file and temp file do not exist");
            return false;
        }

        //check every node of the xml
        return doc.SelectSingleNode("//" + settingName) != null;
    }

    //don't remove method! Method generally called in CompareReadSettingsWithModel()
    private void MergeSettings<T>(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
    {
        Setting<T> mergedSetting = ((Setting<T>)modelField.GetValue(_settings));
        Setting<T> readSetting = ((Setting<T>)readField.GetValue(_settingsReadFromXml));
        mergedSetting.Value = readSetting.Value;
        mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
    }

#if UNITY_IOS

	private void MergeSettings_bool(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
	{
		Setting<bool> mergedSetting = ((Setting<bool>)modelField.GetValue(_settings));
		Setting<bool> readSetting = ((Setting<bool>)readField.GetValue(_settingsReadFromXml));
		mergedSetting.Value = readSetting.Value;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void MergeSettings_string(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
	{
		Setting<string> mergedSetting = ((Setting<string>)modelField.GetValue(_settings));
		Setting<string> readSetting = ((Setting<string>)readField.GetValue(_settingsReadFromXml));
		mergedSetting.Value = readSetting.Value;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void MergeSettings_float(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
	{
		Setting<float> mergedSetting = ((Setting<float>)modelField.GetValue(_settings));
		Setting<float> readSetting = ((Setting<float>)readField.GetValue(_settingsReadFromXml));
		mergedSetting.Value = readSetting.Value;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void MergeSettings_vector2(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
	{
		Setting<Vector2> mergedSetting = ((Setting<Vector2>)modelField.GetValue(_settings));
		Setting<Vector2> readSetting = ((Setting<Vector2>)readField.GetValue(_settingsReadFromXml));
		mergedSetting.Value = readSetting.Value;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void MergeSettings_vector3(ExtendedManagerSettings mergedSettings, FieldInfo modelField, FieldInfo readField)
	{
		Setting<Vector3> mergedSetting = ((Setting<Vector3>)modelField.GetValue(_settings));
		Setting<Vector3> readSetting = ((Setting<Vector3>)readField.GetValue(_settingsReadFromXml));
		mergedSetting.Value = readSetting.Value;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void ApplyDefaultValueOnSetting_bool(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
	{
		Setting<bool> mergedSetting = ((Setting<bool>)modelField.GetValue(_settings));
		mergedSetting.Value = mergedSetting.Defaultvalue;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void ApplyDefaultValueOnSetting_float(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
	{
		Setting<float> mergedSetting = ((Setting<float>)modelField.GetValue(_settings));
		mergedSetting.Value = mergedSetting.Defaultvalue;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void ApplyDefaultValueOnSetting_string(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
	{
		Setting<string> mergedSetting = ((Setting<string>)modelField.GetValue(_settings));
		mergedSetting.Value = mergedSetting.Defaultvalue;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void ApplyDefaultValueOnSetting_vector2(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
	{
		Setting<Vector2> mergedSetting = ((Setting<Vector2>)modelField.GetValue(_settings));
		mergedSetting.Value = mergedSetting.Defaultvalue;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}

	private void ApplyDefaultValueOnSetting_vector3(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
	{
		Setting<Vector3> mergedSetting = ((Setting<Vector3>)modelField.GetValue(_settings));
		mergedSetting.Value = mergedSetting.Defaultvalue;
		mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
	}
#endif

    //don't remove method! Method generically called in CompareReadSettingsWithModel()
    private void ApplyDefaultValueOnSetting<T>(ExtendedManagerSettings mergedSettings, FieldInfo modelField)
    {
        Setting<T> mergedSetting = ((Setting<T>)modelField.GetValue(_settings));
        mergedSetting.Value = mergedSetting.Defaultvalue;
        mergedSettings.GetType().GetField(modelField.Name).SetValue(mergedSettings, mergedSetting);
    }

    private static void SetCursorVisiblity(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    private static void SetFullscreenMode(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private static void SetScreenResolution(Vector2 resolution)
    {
#if UNITY_STANDALONE
        if (resolution.x > 0 && resolution.y > 0)
        {
            Screen.SetResolution((int)resolution.x, (int)resolution.y, Settings.Fullscreen.Value);
        } 
#endif
    }

    public static event PropertyChangedEventHandler PropertyChanged;
    protected static void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(_instance, new PropertyChangedEventArgs(propertyName));
        }
    }
}