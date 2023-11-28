using System.Collections.Generic;
using EasyLog.Core;
using UnityEngine;

namespace EasyLog.Trackers
{
    [AddComponentMenu("EasyLog/Manual Tracker")]
    public class ManualTracker : Tracker
    {
        public static ManualTracker Current => _current;
        private static ManualTracker _current;
        
        private static ManualChannel _standardChannel = new ();
        [HideInInspector] public List<ManualChannel> channels = new() { _standardChannel };
        
        private void Awake()
        {
            if (_current == null)
                _current = this;

            else if (_current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Manual Trackers found! Make sure to only use one tracker of each type! Now removing excess trackers...");
                Destroy(this);
            }

            Initialize();
        }
        
        public ManualChannel GetChannel(int channelIndex = 0)
        {
            if (channels.Count == 0 || channelIndex == 0)
                return _standardChannel;
            
            return channels[channelIndex];
        }

        private void OnApplicationQuit()
        {
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
