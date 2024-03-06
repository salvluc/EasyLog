using System;
using System.IO;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public class SystemInfoWriter : OutputModule
    {
        public string filePrefix = "Log_";
        public string fileSuffix = "_SystemInfo";
        public string saveLocation = Application.dataPath;

        public override string RequiredDataType { get; protected set; } = "INFLUX";

        public override void OnOutputRequested(string influxData, string channelName)
        {
            FileUtility.SaveFile(GetFilePath(), LogUtility.GetSystemInfo());
        }
        
        private string GetFilePath()
        {
            string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{filePrefix}{formattedDateTime}{fileSuffix}";
            string filePath = Path.Combine(saveLocation, fileName);
            return $"{filePath}.txt";
        }
    }
}
