using TMPro;
using System;
using System.IO;
using UnityEngine;
using Settings.Pagination;
using System.Text.RegularExpressions;

namespace Settings.LogMessages
{
    public class LogMessagesOverlay : MonoBehaviour
    {
        public GameObject LogTextsObject;

        private string _logFilePath;
        private SettingsManagerPagesController _settingsManagerPagesController;

        private void OnEnable()
        {
            _logFilePath = GetLogFilePath();
            _settingsManagerPagesController = GetComponentInParent<SettingsManagerPagesController>();
            _settingsManagerPagesController.PropertyChanged += SettingsManagerPagesController_PropertyChanged;
            RefreshLogMessagesButton.RefreshLogsButtonClickEvent += RefreshLogMessagesButton_RefreshLogsButtonClickEvent;
        }

        private void OnDisable()
        {
            RefreshLogMessagesButton.RefreshLogsButtonClickEvent -= RefreshLogMessagesButton_RefreshLogsButtonClickEvent;
        }

        private void SettingsManagerPagesController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedPage")
            {
                if (_settingsManagerPagesController.SelectedPage == SettingsManagerPagesController.SettingsManagerPages.LogMessages)
                {
                    PrintLogsToScrollview();
                }
            }
        }

        private string GetLogFilePath()
        {
            //first try the localpath
            string filepath = Path.Combine(Application.dataPath, "output_log.txt");
            if (File.Exists(filepath))
            {
                return filepath;
            }

            //try the Unity 2017 filelocation
            filepath = Path.Combine(Application.persistentDataPath, "output_log.txt");
            if (File.Exists(filepath))
            {
                return filepath;
            }

            //logfile not found
            return null;
        }

        private void RefreshLogMessagesButton_RefreshLogsButtonClickEvent(object sender, EventArgs e)
        {
            PrintLogsToScrollview();
        }

        private void PrintLogsToScrollview()
        {
            string textToDisplay = "Logfile on " + DateTime.Now + ":  \n\n";

            if (File.Exists(_logFilePath))
            {
                bool isLineErrorMessage = false;

                FileMode fm = FileMode.Open;
                FileStream fs = new FileStream(_logFilePath, fm, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                if (fs != null && sr != null)
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        if (line.Contains("Exception")) //makes exception messages more visible
                        {
                            isLineErrorMessage = true;
                        }

                        if (isLineErrorMessage)
                        {
                            if (!Regex.IsMatch(line, @"[a-zA-Z]")) //detect newline (no letters in line)
                            {
                                isLineErrorMessage = false;
                            }
                            else
                            {
                                line = "<color=red>" + line + "</color>";
                            }
                        }

                        textToDisplay += "\n" + line;
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                textToDisplay = "Log file not present in editor";
#else
                textToDisplay = "Log file not found.";
#endif
            }

            LogTextsObject.GetComponent<TextMeshProUGUI>().text = textToDisplay;
        }
    }
}