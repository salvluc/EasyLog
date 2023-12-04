using System;
using System.IO;
using UnityEngine;

namespace EasyLog.Core
{
    public class Tracker : MonoBehaviour
    {
        public enum TrackerMode { Simple, MultiChannel }
        [HideInInspector] public TrackerMode trackerMode = TrackerMode.Simple;
        
        [HideInInspector] public string filePrefix = "Log";
        [HideInInspector] public string fileSuffix;
        [HideInInspector] public string saveLocation = Application.dataPath;
        [HideInInspector] public char delimiter = ',';
        [HideInInspector] public char delimiterReplacement = '.';

        //private static Channel _standardChannel = new ();
        //[HideInInspector] public List<Channel> channels = new() { _standardChannel };
        
        [HideInInspector] public string _filePath;

        protected virtual void Initialize()
        {
            // format current date and time
            string dateTimeFormat = "dd-MM-yyyy_HH-mm";
            string formattedDateTime = DateTime.Now.ToString(dateTimeFormat);
            string fileName = $"{filePrefix}_{formattedDateTime}_{fileSuffix}.csv";

            _filePath = Path.Combine(saveLocation, fileName);
        }
    }
}
