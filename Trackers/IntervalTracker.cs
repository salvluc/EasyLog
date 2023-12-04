using System.Collections.Generic;
using EasyLog.Core;
using UnityEngine;

namespace EasyLog.Trackers
{
    [AddComponentMenu("EasyLog/Interval Tracker")]
    public class IntervalTracker : Tracker
    {
        public static IntervalTracker Current => _current;
        private static IntervalTracker _current;
        
        [HideInInspector] public IntervalChannel _standardChannel = new IntervalChannel();
        [HideInInspector] public List<IntervalChannel> channels = new List<IntervalChannel>();
        
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

        public void Start()
        {
            if (trackerMode == TrackerMode.Simple)
            {
                StartCoroutine(GetChannel().InitializeLogging());
                return;
            }

            foreach (IntervalChannel channel in channels)
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
            
            foreach (IntervalChannel channel in channels)
            {
                channel.Initialize();
            }
        }
        
        public IntervalChannel GetChannel(int channelIndex = 0)
        {
            if (channels.Count > channelIndex)
                return channels[channelIndex];
            
            Debug.LogError("This channel does not exist. Instead returning the highest channel.");
            return channels[ChannelCount()-1];
        }

        public int ChannelCount()
        {
            return channels.Count;
        }

        private void OnApplicationQuit()
        {
            Debug.Log("EasyLog: Successfully saved logs at: " + saveLocation);
        }
    }
}
