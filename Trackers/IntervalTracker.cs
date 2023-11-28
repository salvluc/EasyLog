using System;
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
        
        [HideInInspector] public IntervalChannel _standardChannel = new ();
        [HideInInspector] public List<IntervalChannel> channels = new();
        
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
            foreach (IntervalChannel channel in channels)
            {
                StartCoroutine(channel.InitializeLogging());
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            channels.Add(_standardChannel);

            foreach (IntervalChannel channel in channels)
            {
                channel.Initialize();
            }
        }
        
        public IntervalChannel GetChannel(int channelIndex = 0)
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
