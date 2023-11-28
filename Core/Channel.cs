using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyLog.Core
{
    public class Channel
    {
        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new();
        private Dictionary<string, Func<string>> _trackedPropertiesViaCode = new();
        
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;

        public Tracker ParentTracker;
        
        protected bool _initialized;
        
        protected bool _hasBeenStarted;
        
        /// <summary>
        /// Starts tracking the specified property in a new column.
        /// </summary>
        /// <param name="propertyAccessor">The property to track, has to be handed over as a Func: "() => property".</param>
        /// <param name="propertyName">The name the property will be saved under.</param>
        public void AddNewProperty(Func<object> propertyAccessor, string propertyName)
        {
            if (!_initialized)
            {
                Debug.LogWarning("EasyLog: You can not add properties before Start()! Add properties in the Inspector or in Start()!");
                return;
            }
            
            if (_hasBeenStarted)
            {
                Debug.LogWarning("EasyLog: You can not add new properties during runtime! Add properties in the Inspector or in Start()!");
                return;
            }
            
            if (_trackedPropertiesViaCode.ContainsKey(propertyName))
                Debug.LogWarning("EasyLog: Cannot add \"" + propertyName + "\" because a property with the same name is already being tracked.");
            else
                _trackedPropertiesViaCode[propertyName] = () => Convert.ToString(propertyAccessor());
        }
        
        protected void WriteHeaders()
        {
            List<string> headers = new List<string>();

            headers.Add("Time");

            // add headers from inspector-based properties
            foreach (var trackedVar in trackedPropertiesViaEditor)
            {
                headers.Add(trackedVar.component.gameObject.name + " " + trackedVar.component.GetType().Name + " " + trackedVar.propertyName);
            }

            // add headers from code-based properties
            headers.AddRange(_trackedPropertiesViaCode.Keys);

            string headerLine = string.Join(",", headers);
            File.WriteAllText(ParentTracker._filePath, headerLine + Environment.NewLine);
        }
        
        protected void WriteValues()
        {
            StringBuilder logLine = new StringBuilder();

            logLine.Append(GetFormattedTime() + ParentTracker.delimiter);

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
                    value = value.Replace(ParentTracker.delimiter, ParentTracker.delimiterReplacement);

                    logLine.Append(value + ",");
                }
            }

            // add values tracked via code
            foreach (var func in _trackedPropertiesViaCode.Values)
            {
                string value = func.Invoke();
                
                // replace commas with dots to prevent delimiter issues
                value = value.Replace(ParentTracker.delimiter, ParentTracker.delimiterReplacement);
                
                logLine.Append(value + ParentTracker.delimiter);
            }

            // remove last delimiter
            if (logLine.Length > 0)
                logLine.Length--;

            // write to .csv file
            File.AppendAllText(ParentTracker._filePath, logLine + Environment.NewLine);
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
