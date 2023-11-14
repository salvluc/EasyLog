using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EasyLog.Components
{
    [AddComponentMenu("EasyLog/Simple Tracker")]
    public class SimpleTracker : MonoBehaviour
    {
        private static SimpleTracker _current;
        public static SimpleTracker Current => _current;

        public enum TrackOption { Interval, Event }
        [HideInInspector] public string filePrefix = "Log";
        [HideInInspector] public string saveLocation = Application.dataPath;
        [HideInInspector] public TrackOption trackMethod = TrackOption.Interval;

        [HideInInspector] public int logsPerSecond = 1;

        [HideInInspector] public List<TrackedProperty> trackedPropertiesViaEditor = new List<TrackedProperty>();
        private Dictionary<string, Func<string>> _trackedPropertiesViaCode = new Dictionary<string, Func<string>>();

        private string _filePath;

        private void Awake()
        {
            if (_current == null)
                _current = this;

            else if (_current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Trackers found! Make sure to only use one tracker! Now removing excess trackers...");
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

            StartCoroutine(InitializeLogging());
        }

        private IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);

            WriteHeader();

            yield return null;

            if (trackMethod == TrackOption.Interval)
                StartCoroutine(TrackByInterval());
        }

        /// <summary>
        /// Starts tracking the specified property.
        /// </summary>
        /// <param name="propertyName">The name the property will be saved under.</param>
        /// <param name="propertyAccessor">The property to track, has to be handed over as a Func: "() => property".</param>
        public void Add(string propertyName, Func<object> propertyAccessor)
        {
            _trackedPropertiesViaCode[propertyName] = () => Convert.ToString(propertyAccessor());
        }

        /// <summary>
        /// Saves all values that are being tracked to the log file.
        /// </summary>
        public void LogEvent()
        {
            WriteValues();
        }

        /// <summary>
        /// Saves all values that are being tracked to the log file.
        /// </summary>
        /// /// &lt;param name="propertyName"&gt;The name the property will be saved under.&lt;/param&gt;
        public void LogEvent(string eventName)
        {
            WriteValues(eventName);
        }

        private void WriteHeader()
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

        private IEnumerator TrackByInterval()
        {
            while (true)
            {
                WriteValues();
                yield return new WaitForSeconds(1f / logsPerSecond);
            }
        }

        private void WriteValues(string eventName = "")
        {
            StringBuilder logLine = new StringBuilder();

            logLine.Append(GetFormattedTime() + ",");

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
                    value = value.Replace(',', '.');

                    logLine.Append(value + ",");
                }
            }

            // add values tracked via code
            foreach (var func in _trackedPropertiesViaCode.Values)
            {
                logLine.Append(func.Invoke() + ",");
            }

            if (eventName != "")
                logLine.Append(eventName + ",");

            // remove the last comma
            if (logLine.Length > 0)
                logLine.Length--;

            // write to .csv file
            File.AppendAllText(_filePath, logLine + Environment.NewLine);
        }

        string GetFormattedTime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D}",
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds);

            return formattedTime;
        }

        private void OnApplicationQuit()
        {
            // final save on application quit
            WriteValues();
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
