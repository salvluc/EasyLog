using System;
using System.Collections.Generic;
using EasyLog.Output;
using UnityEngine;
using UnityEngine.Serialization;

namespace EasyLog.Core
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

        public void Start()
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

        private void OnApplicationQuit()
        {
            foreach (var outputModule in outputModules)
            {
                foreach (Channel channel in channels)
                {
                    outputModule.OnOutputRequested(outputModule.RequiredDataType == "CSV" ? channel.DataSet.SerializeForCSV() : channel.DataSet.SerializeForInflux());
                }
            }
        }
    }
}
