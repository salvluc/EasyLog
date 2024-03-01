using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EasyLog.Core
{
    [Serializable]
    public class Channel
    {
        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new();
        private Dictionary<string, Func<string>> _trackedPropertiesViaCode = new();
        
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;
        
        public enum IntervalOption { Seconds, PerSecond }
        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        
        [HideInInspector] public int logInterval = 1;

        [HideInInspector] public Tracker Tracker;
        [HideInInspector] public int ChannelIndex;
    
        private DataSet DataSet = new();
        private bool Initialized;
        
        private float DelayBetweenLogs => intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        
        public IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            //yield return new WaitForSeconds(0.1f);
            yield return new WaitForEndOfFrame();
            Initialized = true;
            Tracker.StartCoroutine(TrackByInterval());
        }
        
        private IEnumerator TrackByInterval()
        {
            while (true)
            {
                if (logInterval == 0) continue;
                    
                CaptureValues();
                
                if (timeScaleOption == TimeScaleOption.Scaled)
                    yield return new WaitForSeconds(DelayBetweenLogs);
                else
                    yield return new WaitForSecondsRealtime(DelayBetweenLogs);
            }
        }
        
        /// <summary>
        /// Starts tracking the specified property.
        /// </summary>
        /// <param name="propertyAccessor">The property to track, has to be handed over as a Func: "() => property".</param>
        /// <param name="propertyName">The name the property will be saved under.</param>
        public void StartTrackingProperty(Func<object> propertyAccessor, string propertyName)
        {
            if (!Initialized)
            {
                Debug.LogWarning("EasyLog: You can not add properties before Start()! Add properties in the Inspector or in Start()!");
                return;
            }
            Debug.Log("START TRACKING: " + propertyName);
            
            if (_trackedPropertiesViaCode.ContainsKey(propertyName))
                Debug.LogWarning("EasyLog: Cannot add \"" + propertyName + "\" because a property with the same name is already being tracked.");
            else
                _trackedPropertiesViaCode[propertyName] = () => Convert.ToString(propertyAccessor());
        }
        
        /// <summary>
        /// Stops tracking the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public void StopTrackingProperty(string propertyName)
        {
            if (!Initialized)
            {
                Debug.LogWarning("EasyLog: You can not remove properties before Start()! Remove properties in the Inspector or in Start()!");
                return;
            }
            
            Debug.Log("STOP TRACKING: " + propertyName);
            
            if (!_trackedPropertiesViaCode.Remove(propertyName))
                Debug.LogWarning("EasyLog: Cannot find property \"" + propertyName + "\".");
        }
        
        /// <summary>
        /// Manually logs a value.
        /// </summary>
        /// <param name="name">The name of the logged value.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The name of the logged value.</param>
        public void Log(string name, string value, Dictionary<string, string> tags)
        {
            tags["sessionId"] = Tracker.SessionId;
            DataPoint newData = new DataPoint(Application.productName, GetUnixTime(), name, value, tags);
            DataSet.Add(newData);
        }
        
        /// <summary>
        /// Logs the values of all tracked properties.
        /// </summary>
        public void LogAllTrackedProperties()
        {
            if (!Initialized)
            {
                Debug.LogWarning("EasyLog: You can not log before Start()!");
                return;
            }
            
            CaptureValues();
        }
        
        public void SaveDataToDisk()
        {
            if (Tracker.outputFormat == Tracker.OutputFormat.Influx)
                File.WriteAllText(GetChannelFilePath(), DataSet.SerializeForInflux());
            if (Tracker.outputFormat == Tracker.OutputFormat.CSV)
                File.WriteAllText(GetChannelFilePath(), DataSet.SerializeForCSV(Tracker.delimiter, Tracker.delimiterReplacement));
        }
        
        private void CaptureValues()
        {
            foreach (var trackedVar in trackedPropertiesViaEditor)
            {
                if (trackedVar.component == null || string.IsNullOrEmpty(trackedVar.propertyName)) continue;
                
                string value = "";
                PropertyInfo propInfo = trackedVar.component.GetType().GetProperty(trackedVar.propertyName);
                FieldInfo fieldInfo = trackedVar.component.GetType().GetField(trackedVar.propertyName);
                    
                if (propInfo != null)
                    value = propInfo.GetValue(trackedVar.component, null).ToString();

                else if (fieldInfo != null)
                    value = fieldInfo.GetValue(trackedVar.component).ToString();

                Dictionary<string, string> newTags = new Dictionary<string, string>() { {"sessionId", Tracker.SessionId} };
                    
                DataPoint newData = new DataPoint(Application.productName, GetUnixTime(), trackedVar.Name, value, newTags);
                DataSet.Add(newData);
            }

            // add values tracked via code
            foreach (var trackedVar in _trackedPropertiesViaCode)
            {
                string value = trackedVar.Value.Invoke();
                
                Dictionary<string, string> newTags = new Dictionary<string, string>() { {"sessionId", Tracker.SessionId} };
                
                DataPoint newData = new DataPoint(Application.productName, GetUnixTime(), trackedVar.Key, value, newTags);
                DataSet.Add(newData);
            }
        }
        
        private string GetFormattedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            string formattedTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D}";
            return formattedTime;
        }
        
        private string GetUnixTime() // start date is 01-01-2024
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            return ((long)Mathf.Floor((float)timeSpan.TotalSeconds) + 1704067200).ToString();
        }

        private string GetChannelFilePath()
        {
            string fileEnding = Tracker.outputFormat == Tracker.OutputFormat.Influx ? ".txt" : ".csv";
            return $"{Tracker._filePath.Remove(Tracker._filePath.Length - 3)}_Channel{ChannelIndex}{fileEnding}";
        }
    }
}
