using System;
using System.Collections;
using EasyLog.Core;
using UnityEngine;

namespace EasyLog.Trackers
{
    [AddComponentMenu("EasyLog/Manual Tracker")]
    public class ManualTracker : Tracker
    {
        public static ManualTracker Current => _current;
        private static ManualTracker _current;

        [HideInInspector] public bool logOnStart;
        
        private static bool _hasBeenStarted;

        private void Awake()
        {
            if (_current == null)
                _current = this;

            else if (_current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Manual Trackers found! Make sure to only use one tracker of each type! Now removing excess trackers...");
                Destroy(this);
            }
        }

        private void Start()
        {
            Initialize();
            
            StartCoroutine(InitializeLogging());
        }

        private IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);

            _hasBeenStarted = true;

            WriteHeaders();
            
            if (logOnStart)
                WriteValues();
        }

        /// <summary>
        /// Starts tracking the specified property in a new column.
        /// </summary>
        /// <param name="propertyAccessor">The property to track, has to be handed over as a Func: "() => property".</param>
        /// <param name="propertyName">The name the property will be saved under.</param>
        public void TrackNewProperty(Func<object> propertyAccessor, string propertyName)
        {
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
        
        /// <summary>
        /// Logs the values of all tracked properties.
        /// </summary>
        public void LogAllTrackedProperties()
        {
            WriteValues();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
