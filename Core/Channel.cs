using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public class Channel
    {
        public DataSet DataSet { get; private set; } = new();
        
        [HideInInspector] public List<TrackedEditorProperty> trackedPropertiesViaEditor = new();
        [HideInInspector] public List<TrackedCodeProperty> trackedPropertiesViaCode = new();

        [HideInInspector] public string measurementName = "gameMeasurement";
        
        public enum IntervalOption { Seconds, PerSecond }
        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;
        
        [HideInInspector] public int logInterval = 1;

        [HideInInspector] public bool systemInfoAsTags = true;
        
        [HideInInspector] public Tracker tracker;
        [HideInInspector] public int channelIndex;
        
        private float DelayBetweenLogs => intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        
        private bool _initialized;
        
        public IEnumerator InitializeLogging()
        {
            yield return null; // wait to ensure all code-based variables are registered
            _initialized = true;
            tracker.StartCoroutine(TrackByInterval());
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
            if (!_initialized)
            {
                Debug.LogWarning("EasyLog: You can not add properties before Start()! Add properties in the Inspector or in Start()!");
                return;
            }
            Debug.Log("START TRACKING: " + propertyName);

            if (trackedPropertiesViaCode.Any(codeProperty => codeProperty.Name == propertyName || codeProperty.Accessor == propertyAccessor))
            {
                Debug.LogWarning("EasyLog: Cannot add \"" + propertyName + "\" because a property with the same name is already being tracked.");
                return;
            }

            var newCodeProperty = new TrackedCodeProperty
            {
                Accessor = propertyAccessor,
                Name = propertyName
            };
            
            trackedPropertiesViaCode.Add(newCodeProperty);
        }
        
        /// <summary>
        /// Stops tracking the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public void StopTrackingProperty(string propertyName)
        {
            if (!_initialized)
            {
                Debug.LogWarning("EasyLog: You can not remove properties before Start()! Remove properties in the Inspector or in Start()!");
                return;
            }
            
            Debug.Log("STOP TRACKING: " + propertyName);
            
            if (trackedPropertiesViaCode.All(codeProperty => codeProperty.Name != propertyName))
            {
                Debug.LogWarning("EasyLog: Cannot find property \"" + propertyName + "\".");
                return;
            }

            foreach (var prop in trackedPropertiesViaCode.Where(codeProperty => codeProperty.Name == propertyName))
            {
                trackedPropertiesViaCode.Remove(prop);
                return;
            }
        }
        
        /// <summary>
        /// Manually logs a value.
        /// </summary>
        /// <param name="name">The name of the logged value.</param>
        /// <param name="value">The value.</param>
        /// <param name="tags">The name of the logged value.</param>
        public void Log(string name, string value, Dictionary<string, string> tags)
        {
            if (systemInfoAsTags)
            {
                foreach (var pair in LogUtility.GetSystemInfoAsTags())
                {
                    tags.TryAdd(pair.Key, pair.Value);
                }
            }
            
            DataPoint newData = new DataPoint(FileUtility.InfluxFormat(measurementName), GetUnixTime(), name, value, AddStandardTags(tags));
            DataSet.Add(newData);
        }
        
        /// <summary>
        /// Logs the values of all tracked properties.
        /// </summary>
        public void LogAllTrackedProperties()
        {
            if (!_initialized)
            {
                Debug.LogWarning("EasyLog: You can not log before Start()!");
                return;
            }
            
            CaptureValues();
        }
        
        private void CaptureValues()
        {
            for (int i = 0; i < trackedPropertiesViaEditor.Count; i++)
            {
                TrackedEditorProperty trackedEditorVar = trackedPropertiesViaEditor[i];
                
                if (trackedEditorVar.component == null || string.IsNullOrEmpty(trackedEditorVar.propertyName))
                {
                    Debug.LogWarning("EasyLog: " + "\"" + trackedEditorVar.propertyName + "\"" + "cannot be found and will be removed from tracker.");
                    trackedPropertiesViaEditor.Remove(trackedEditorVar);
                    continue;
                }
                
                string value = "";
                PropertyInfo propInfo = trackedEditorVar.component.GetType().GetProperty(trackedEditorVar.propertyName);
                FieldInfo fieldInfo = trackedEditorVar.component.GetType().GetField(trackedEditorVar.propertyName);
                    
                if (propInfo != null)
                    value = propInfo.GetValue(trackedEditorVar.component, null).ToString();

                else if (fieldInfo != null)
                    value = fieldInfo.GetValue(trackedEditorVar.component).ToString();

                Dictionary<string, string> newTags = AddStandardTags(new Dictionary<string, string>());

                if (systemInfoAsTags)
                {
                    foreach (var pair in LogUtility.GetSystemInfoAsTags())
                    {
                        newTags.TryAdd(pair.Key, pair.Value);
                    }
                }
                    
                DataPoint newData = new DataPoint(FileUtility.InfluxFormat(measurementName), GetUnixTime(), trackedEditorVar.Name, value, newTags);
                DataSet.Add(newData);
            }

            // add values tracked via code
            foreach (var trackedVar in trackedPropertiesViaCode)
            {
                if (trackedVar.Accessor == null)
                {
                    Debug.LogWarning("EasyLog: " + "\"" + trackedVar.Name + "\"" + "cannot be found and will be removed from tracker.");
                    trackedPropertiesViaCode.Remove(trackedVar);
                    continue;
                }
                
                string value = trackedVar.Accessor.Invoke().ToString();
                
                Dictionary<string, string> newTags = AddStandardTags(new Dictionary<string, string>());
                
                if (systemInfoAsTags)
                {
                    foreach (var pair in LogUtility.GetSystemInfoAsTags())
                    {
                        newTags.TryAdd(pair.Key, pair.Value);
                    }
                }
                
                DataPoint newData = new DataPoint(FileUtility.InfluxFormat(measurementName), GetUnixTime(), trackedVar.Name, value, newTags);
                DataSet.Add(newData);
            }
        }

        private Dictionary<string, string> AddStandardTags(Dictionary<string, string> tags)
        {
            tags["sessionId"] = tracker.SessionId;
            tags["date"] = DateTime.Now.ToString("yyyy-MM-dd");
            //tags["logTime"] = DateTime.Now.ToString("HH:mm:ss");s
            //tags["scaledGameTime"] = GetFormattedTime(TimeScaleOption.Scaled);
            //tags["unscaledGameTime"] = GetFormattedTime(TimeScaleOption.Unscaled);
            
            return tags;
        }
        
        private string GetFormattedTime(TimeScaleOption timeScale)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScale == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            string formattedTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D}";
            return formattedTime;
        }
        
        private string GetUnixTime() // second precision, start date is 01-01-2024 1:00
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            return ((long)Mathf.Floor((float)timeSpan.TotalMilliseconds) + 1704067200000).ToString();
        }
    }
}
