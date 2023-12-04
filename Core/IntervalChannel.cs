using System;
using System.Collections;
using UnityEngine;

namespace EasyLog.Core
{
    [Serializable]
    public class IntervalChannel : Channel
    {
        public enum IntervalOption { Seconds, PerSecond }
        
        [HideInInspector] public IntervalOption intervalOption = IntervalOption.Seconds;
        [HideInInspector] public int logInterval = 1;

        [HideInInspector] public bool startAutomatically = true;
        private bool _isPaused = false;
        private float _delayBetweenLogs;

        public void Initialize()
        {
            _isPaused = !startAutomatically;

            _delayBetweenLogs = DelayBetweenLogs();

            _initialized = true;
        }
        
        private float DelayBetweenLogs()
        {
            return intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        }
        
        public IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);
            
            WriteHeaders();
            
            _hasBeenStarted = true;
            
            if (startAutomatically)
                ParentTracker.StartCoroutine(TrackByInterval());
        }
        
        private IEnumerator TrackByInterval()
        {
            while (true)
            {
                if (_isPaused)
                    continue;
                    
                WriteValues();
                
                if (timeScaleOption == TimeScaleOption.Scaled)
                    yield return new WaitForSeconds(_delayBetweenLogs);
                else
                    yield return new WaitForSecondsRealtime(_delayBetweenLogs);
            }
        }
        
        public void Pause()
        {
            _isPaused = true;
        }
        
        public void Unpause()
        {
            _isPaused = false;
        }
    }
}
