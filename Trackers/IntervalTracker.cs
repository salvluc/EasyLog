using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyLog.Trackers
{
    [AddComponentMenu("EasyLog/Interval Tracker")]
    public class IntervalTracker : MonoBehaviour
    {
        private static IntervalTracker _current;

        public enum IntervalOption { Seconds, PerSecond }
        public enum TimeScaleOption { Scaled, Unscaled }

        [HideInInspector] public string filePrefix = "Log";
        [HideInInspector] public string saveLocation = Application.dataPath;
        [HideInInspector] public char delimiter = ',';
        [HideInInspector] public char delimiterReplacement = '.';

        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        [HideInInspector] public int logInterval = 1;
        [HideInInspector] public TimeScaleOption timeScaleOption = TimeScaleOption.Scaled;
        [HideInInspector] public bool startAutomatically = true;
        private static bool _isPaused = false;
        private float _delayBetweenLogs;

        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new List<TrackedProperty>();
        private static Dictionary<string, Func<string>> _trackedPropertiesViaCode = new Dictionary<string, Func<string>>();

        private string _filePath;

        private void Awake()
        {
            if (_current == null)
                _current = this;

            else if (_current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Interval Trackers found! Make sure to only use one tracker of each type! Now removing excess trackers...");
                Destroy(this);
            }
        }

        private void Start()
        {
            // format current date and time
            string dateTimeFormat = "dd-MM-yyyy_HH-mm";
            string formattedDateTime = DateTime.Now.ToString(dateTimeFormat);
            string fileName = $"{filePrefix}_{formattedDateTime}.csv";

            _filePath = Path.Combine(saveLocation, fileName);

            _isPaused = !startAutomatically;

            _delayBetweenLogs = DelayBetweenLogs();

            StartCoroutine(InitializeLogging());
        }

        private IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);

            WriteHeaders();

            yield return null;
            
            StartCoroutine(TrackByInterval());
        }

        private float DelayBetweenLogs()
        {
            return intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        }

        /// <summary>
        /// Starts tracking the specified property in a new column.
        /// </summary>
        /// <param name="propertyAccessor">The property to track, has to be handed over as a Func: "() => property".</param>
        /// <param name="propertyName">The name the property will be saved under.</param>
        public static void TrackNewProperty(Func<object> propertyAccessor, string propertyName)
        {
            if (_trackedPropertiesViaCode.ContainsKey(propertyName))
                Debug.LogWarning("Cannot add \"" + propertyName + "\" because a property with the same name is already being tracked.");
            else
                _trackedPropertiesViaCode[propertyName] = () => Convert.ToString(propertyAccessor());
        }

        public static void Pause()
        {
            _isPaused = true;
        }
        
        public static void Unpause()
        {
            _isPaused = false;
        }

        private IEnumerator TrackByInterval()
        {
            while (true)
            {
                yield return new WaitUntil(() => !_isPaused);
                
                WriteValues();
                
                if (timeScaleOption == TimeScaleOption.Scaled)
                    yield return new WaitForSeconds(_delayBetweenLogs);
                else
                    yield return new WaitForSecondsRealtime(_delayBetweenLogs);
            }
        }
        
        private void WriteHeaders()
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

            headers.Add("Event");

            string headerLine = string.Join(",", headers);
            File.WriteAllText(_filePath, headerLine + Environment.NewLine);
        }

        private void WriteValues(string eventName = "")
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

            if (eventName != "")
                logLine.Append(eventName + delimiter);

            // remove the last comma
            if (logLine.Length > 0)
                logLine.Length--;

            // write to .csv file
            File.AppendAllText(_filePath, logLine + Environment.NewLine);
        }

        string GetFormattedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeScaleOption == TimeScaleOption.Scaled ? Time.time : Time.unscaledTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D}",
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds);

            return formattedTime;
        }

        private void OnApplicationQuit()
        {
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
