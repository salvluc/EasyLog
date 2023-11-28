using System;
using System.Collections;
using UnityEngine;

namespace EasyLog.Trackers
{
    [AddComponentMenu("EasyLog/Interval Tracker")]
    public class IntervalTracker : Core.Tracker
    {
        public static IntervalTracker Current => _current;
        private static IntervalTracker _current;

        public enum IntervalOption { Seconds, PerSecond }
        
        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        [HideInInspector] public int logInterval = 1;

        [HideInInspector] public bool startAutomatically = true;
        private static bool _isPaused = false;
        private float _delayBetweenLogs;
        
        private static bool _hasBeenStarted;

        private void Awake()
        {
            if (_current == null)
                _current = this;

            else if (_current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Interval Trackers found! Make sure to only use one tracker of each type! Now removing excess trackers...");
                Destroy(this);
            }
            
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            _isPaused = !startAutomatically;

            _delayBetweenLogs = DelayBetweenLogs();
        }

        private void Start()
        {
            StartCoroutine(InitializeLogging());
        }

        private IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);
            
            WriteHeaders();
            
            _hasBeenStarted = true;
            
            if (startAutomatically)
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

        private void OnApplicationQuit()
        {
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
