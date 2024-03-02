using System;
using System.IO;
using UnityEngine;

namespace EasyLog.Output
{
    [Serializable]
    public class InfluxWriter : OutputModule
    {
        public string filePrefix = "Log_";
        public string fileSuffix;
        public string saveLocation = Application.dataPath;

        public override string RequiredDataType { get; protected set; } = "INFLUX";
        
        private string _channelName;

        public override void OnOutputRequested(string influxData)
        {
            File.WriteAllText(GetFilePath(), influxData);
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
            return $"{filePath}{_channelName}.txt";
        }
    }
}
