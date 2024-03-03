using System;
using System.IO;
using UnityEngine;

namespace EasyLog
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
            FileUtility.SaveFile(GetFilePath(), influxData);
        }
        
        public void SetChannelName(string channelName)
        {
            _channelName = channelName;
        }
        
        private string GetFilePath()
        {
            string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{filePrefix}{formattedDateTime}{fileSuffix}";
            string filePath = Path.Combine(saveLocation, fileName);
            return $"{filePath}{_channelName}.txt";
        }
    }
}
