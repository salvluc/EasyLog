using System;
using System.IO;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public class CSVWriter : OutputModule
    {
        public string filePrefix = "Log_";
        public string fileSuffix;
        public string saveLocation = Application.dataPath;
        public char delimiter = ',';
        public char delimiterReplacement = '.';

        private string _channelName;
        
        public override string RequiredDataType { get; protected set; } = "CSV";

        public override void OnOutputRequested(string csvData)
        {
            File.WriteAllText(GetFilePath(), csvData);
        }

        public void SetChannelName(string channelName)
        {
            _channelName = channelName;
        }
        
        private string GetFilePath()
        {
            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy_HH-mm");
            string fileName = $"{filePrefix}{formattedDateTime}{fileSuffix}";
            string filePath = Path.Combine(saveLocation, fileName);
            return $"{filePath}{_channelName}.csv";
        }
    }
}
