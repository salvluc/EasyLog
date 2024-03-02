using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace EasyLog.Core
{
    [Serializable]
    public class Channel
    {
        [HideInInspector] public List<TrackedEditorProperty> trackedPropertiesViaEditor = new();
        [HideInInspector] public List<TrackedCodeProperty> trackedPropertiesViaCode = new();
        
        public enum TimeScaleOption { Scaled, Unscaled }
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;
        
        public enum IntervalOption { Seconds, PerSecond }
        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        
        [HideInInspector] public int logInterval = 1;

        [HideInInspector] public Tracker Tracker;
        [HideInInspector] public int ChannelIndex;
    
        public DataSet DataSet { get; private set; } = new();
        private bool Initialized;
        
        private float DelayBetweenLogs => intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        
        public IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            //yield return new WaitForSeconds(0.1f);
            yield return null;
            //yield return new WaitForEndOfFrame();
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
            if (!Initialized)
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

                Dictionary<string, string> newTags = new Dictionary<string, string>() { {"sessionId", Tracker.SessionId} };
                    
                DataPoint newData = new DataPoint(Application.productName, GetUnixTime(), trackedEditorVar.Name, value, newTags);
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
                
                Dictionary<string, string> newTags = new Dictionary<string, string>() { {"sessionId", Tracker.SessionId} };
                
                DataPoint newData = new DataPoint(Application.productName, GetUnixTime(), trackedVar.Name, value, newTags);
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
    }
}
