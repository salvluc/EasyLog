using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyLog.Core
{
    public class Tracker : MonoBehaviour
    {
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;
        
        [HideInInspector] public string filePrefix = "Log";
        [HideInInspector] public string fileSuffix;
        [HideInInspector] public string saveLocation = Application.dataPath;
        [HideInInspector] public char delimiter = ',';
        [HideInInspector] public char delimiterReplacement = '.';
        
        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new List<TrackedProperty>();
        protected Dictionary<string, Func<string>> _trackedPropertiesViaCode = new Dictionary<string, Func<string>>();
        
        private string _filePath;
        
        private void Start()
        {
            // format current date and time
            string dateTimeFormat = "dd-MM-yyyy_HH-mm";
            string formattedDateTime = DateTime.Now.ToString(dateTimeFormat);
            string fileName = $"{filePrefix}_{formattedDateTime}_{fileSuffix}.csv";

            _filePath = Path.Combine(saveLocation, fileName);
        }

        protected void WriteHeaders()
        {
            List<string> headers = new List<string>();

            headers.Add("Time");

            // add headers from inspector-based properties
            foreach (var trackedVar in trackedPropertiesViaEditor)
            {
                headers.Add(trackedVar.component.gameObject.name + " " + trackedVar.component.name + " " + trackedVar.propertyName);
            }

            // add headers from code-based properties
            headers.AddRange(_trackedPropertiesViaCode.Keys);

            string headerLine = string.Join(",", headers);
            File.WriteAllText(_filePath, headerLine + Environment.NewLine);
        }
        
        protected void WriteValues()
        {
            StringBuilder logLine = new StringBuilder();

            logLine.Append(GetFormattedTime() + delimiter);

            // add values tracked via inspector
            foreach (var trackedVar in trackedPropertiesViaEditor)
            {
                if (trackedVar.component != null && !string.IsNullOrEmpty(trackedVar.propertyName))
                {
                    PropertyInfo propInfo = trackedVar.component.GetType().GetProperty(trackedVar.propertyName);
                    FieldInfo fieldInfo = trackedVar.component.GetType().GetField(trackedVar.propertyName);

                    string value = "";

                    if (propInfo != null)
                        value = propInfo.GetValue(trackedVar.component, null).ToString();

                    else if (fieldInfo != null)
                        value = fieldInfo.GetValue(trackedVar.component).ToString();

                    // replace commas with dots to prevent delimiter issues
                    value = value.Replace(delimiter, delimiterReplacement);

                    logLine.Append(value + ",");
                }
            }

            // add values tracked via code
            foreach (var func in _trackedPropertiesViaCode.Values)
            {
                string value = func.Invoke();
                
                // replace commas with dots to prevent delimiter issues
                value = value.Replace(delimiter, delimiterReplacement);
                
                logLine.Append(value + delimiter);
            }

            // remove last delimiter
            if (logLine.Length > 0)
                logLine.Length--;

            // write to .csv file
            File.AppendAllText(_filePath, logLine + Environment.NewLine);
        }
        
        private string GetFormattedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D}",
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds);

            return formattedTime;
        }
    }
}
