using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EasyLog.Core
{
    [AddComponentMenu("EasyLog/Tracker")]
    public class Tracker : MonoBehaviour
    {
        public enum TrackerMode { Simple, MultiChannel }
        [HideInInspector] public TrackerMode trackerMode = TrackerMode.Simple;
        
        public enum OutputFormat { Influx, CSV }
        [HideInInspector] public OutputFormat outputFormat = OutputFormat.Influx;
        
        [HideInInspector] public string filePrefix = "Log";
        [HideInInspector] public string fileSuffix;
        [HideInInspector] public string saveLocation = Application.dataPath;
        [HideInInspector] public char delimiter = ',';
        [HideInInspector] public char delimiterReplacement = '.';
        
        [HideInInspector] public string _filePath;
        public static Tracker Current { get; private set; }
        public int ChannelCount => channels.Count;
        public string SessionId { get; private set; }
        
        [HideInInspector] public List<Channel> channels = new List<Channel>();
        
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
            // format current date and time
            string dateTimeFormat = "dd-MM-yyyy_HH-mm";
            string formattedDateTime = DateTime.Now.ToString(dateTimeFormat);
            string fileName = $"{filePrefix}_{formattedDateTime}_{fileSuffix}.csv";

            _filePath = Path.Combine(saveLocation, fileName);
            
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
            foreach (Channel channel in channels)
            {
                channel.SaveDataToDisk();
            }
            
            Debug.Log("Interval Tracker: Successfully saved log(s) at: " + saveLocation);
        }
    }
}
