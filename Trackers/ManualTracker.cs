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
        
        public void Start()
        {
            if (trackerMode == TrackerMode.Simple)
            {
                StartCoroutine(GetChannel().InitializeLogging());
                return;
            }

            foreach (ManualChannel channel in channels)
            {
                StartCoroutine(channel.InitializeLogging());
            }
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            
            if (trackerMode == TrackerMode.Simple)
            {
                GetChannel().Initialize();
                return;
            }
            
            foreach (ManualChannel channel in channels)
            {
                channel.Initialize();
            }
        }
        
        public ManualChannel GetChannel(int channelIndex = 0)
        {
            if (trackerMode == TrackerMode.Simple && channelIndex > 0)
            {
                Debug.LogWarning("Tracker is not in Multi-Channel mode. Instead returning the standard channel.");
                return channels[0];
            }
            
            if (ChannelCount() > channelIndex)
                return channels[channelIndex];
            
            Debug.LogWarning("This channel does not exist. Instead returning the highest channel.");
            return channels[ChannelCount()-1];
        }
        
        public int ChannelCount()
        {
            return channels.Count;
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("Manual Tracker: Successfully saved log(s) at: " + saveLocation);
        }
    }
}
