using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EasyLog.Core
{
    [Serializable]
    public class Channel
    {
        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new List<TrackedProperty>();
        private Dictionary<string, Func<string>> _trackedPropertiesViaCode = new Dictionary<string, Func<string>>();
        
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;

        protected DataSet DataSet = new DataSet();

        public Tracker ParentTracker;

        public int ChannelIndex = 0;
        
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
        
        protected void CaptureValues()
        {
            foreach (var trackedVar in trackedPropertiesViaEditor)
            {
                if (trackedVar.component != null && !string.IsNullOrEmpty(trackedVar.propertyName))
                {
                    string value = "";
                    
                    PropertyInfo propInfo = trackedVar.component.GetType().GetProperty(trackedVar.propertyName);
                    FieldInfo fieldInfo = trackedVar.component.GetType().GetField(trackedVar.propertyName);
                    
                    if (propInfo != null)
                        value = propInfo.GetValue(trackedVar.component, null).ToString();

                    else if (fieldInfo != null)
                        value = fieldInfo.GetValue(trackedVar.component).ToString();
                    
                    DataPoint newData = new DataPoint(trackedVar.Name, GetFormattedTime(), value);
                    DataSet.Add(newData);
                }
            }

            // add values tracked via code
            foreach (var trackedVar in _trackedPropertiesViaCode)
            {
                string value = trackedVar.Value.Invoke();
                
                DataPoint newData = new DataPoint(trackedVar.Key, GetFormattedTime(), value);
                DataSet.Add(newData);
            }
        }

        public void SaveDataToDisk()
        {
            if (ParentTracker.outputFormat == Tracker.OutputFormat.Influx)
                File.WriteAllText(GetChannelFilePath(), DataSet.SerializeForInflux());
            if (ParentTracker.outputFormat == Tracker.OutputFormat.CSV)
                File.WriteAllText(GetChannelFilePath(), DataSet.SerializeForCSV(ParentTracker.delimiter, ParentTracker.delimiterReplacement));
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
        
        private float GetUnformattedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            return (float)timeSpan.TotalSeconds;
        }

        private string GetChannelFilePath()
        {
            string fileEnding = ParentTracker.outputFormat == Tracker.OutputFormat.Influx ? ".txt" : ".csv";
            
            return $"{ParentTracker._filePath.Remove(ParentTracker._filePath.Length - 3)}_Channel{ChannelIndex}{fileEnding}";
        }
    }
}
