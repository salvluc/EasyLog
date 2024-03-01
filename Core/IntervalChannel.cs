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
        
        private float DelayBetweenLogs => intervalOption == IntervalOption.Seconds ? logInterval : 1f / logInterval;
        private bool _isPaused;

        public override void Initialize()
        {
            _isPaused = !startAutomatically;
            base.Initialize();
        }
        
        public IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);
            
            if (startAutomatically)
                ParentTracker.StartCoroutine(TrackByInterval());
        }
        
        private IEnumerator TrackByInterval()
        {
            while (true)
            {
                if (_isPaused) continue;
                    
                CaptureValues();
                
                if (timeScaleOption == TimeScaleOption.Scaled)
                    yield return new WaitForSeconds(DelayBetweenLogs);
                else
                    yield return new WaitForSecondsRealtime(DelayBetweenLogs);
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
