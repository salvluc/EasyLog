using System;
using System.Collections;
using UnityEngine;

namespace EasyLog.Core
{
    [Serializable]
    public class ManualChannel : Channel
    {
        private static bool _hasBeenStarted;
        [HideInInspector] public bool logOnStart;
        
        public void Initialize()
        {
            _initialized = true;
        }
        
        public IEnumerator InitializeLogging()
        {
            // wait to ensure all code-based variables are registered
            yield return new WaitForSeconds(0.1f);

            WriteHeaders();

            _hasBeenStarted = true;
            
            if (logOnStart)
                WriteValues();
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
            
            WriteValues();
        }
    }
}
