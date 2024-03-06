using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyLog
{
    [AddComponentMenu("EasyLog/Tracker")]
    public class Tracker : MonoBehaviour
    {
        [HideInInspector] public List<Channel> channels = new();
        [HideInInspector] [SerializeReference] public List<OutputModule> outputModules = new();
        
        public static Tracker Current { get; private set; }
        public int ChannelCount => channels.Count;
        public string SessionId { get; private set; }
        
        public enum TrackerMode { Simple, MultiChannel }
        [HideInInspector] public TrackerMode trackerMode = TrackerMode.Simple;

        private bool _outputTriggered;
        
        private void Awake()
        {
            if (Current == null)
                Current = this;

            else if (Current != this)
            {
                Debug.LogWarning("EasyLog: Multiple Interval Trackers found! Make sure to only use one tracker of each type! Now removing excess trackers...");
                Destroy(this);
            }
            
            Initialize();
        }

        public void OnEnable()
        {
            if (trackerMode == TrackerMode.Simple)
            {
                StartCoroutine(GetChannel().InitializeLogging());
                return;
            }

            foreach (Channel channel in channels)
            {
                StartCoroutine(channel.InitializeLogging());
            }

            _outputTriggered = false;
        }
        
        private void Initialize()
        {
            SessionId = Guid.NewGuid().ToString("n");
        }
        
        public Channel GetChannel(int channelIndex = 0)
        {
            if (trackerMode == TrackerMode.Simple && channelIndex > 0)
            {
                Debug.LogWarning("Tracker is not in Multi-Channel mode. Instead returning the standard channel.");
                return channels[0];
            }
            
            if (ChannelCount > channelIndex)
                return channels[channelIndex];
            
            Debug.LogWarning("This channel does not exist. Instead returning the highest channel.");
            return channels[ChannelCount-1];
        }

        private void OnTriggerOutput()
        {
            if (_outputTriggered) return;

            _outputTriggered = true;
            
            foreach (Channel channel in channels)
            {
                Debug.Log("CHANNEL LOG: " + channel.ChannelIndex);
                
                foreach (var outputModule in outputModules)
                {
                    if (outputModule is CSVWriter csvWriter)
                    {
                        csvWriter.OnOutputRequested(channel.DataSet.SerializeForCsv(csvWriter.delimiter, csvWriter.delimiterReplacement), "Channel" + channel.ChannelIndex);
                        continue;
                    }
                    outputModule.OnOutputRequested(channel.DataSet.SerializeForInflux(), "Channel" + channel.ChannelIndex); // needs to be changed when more csv output modules are added
                }
            }
        }

        private void OnApplicationQuit()
        {
            OnTriggerOutput();
        }

        private void OnDisable()
        {
            OnTriggerOutput();
        }
    }
}
