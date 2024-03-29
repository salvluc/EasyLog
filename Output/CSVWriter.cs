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

        public override void OnOutputRequested(string csvData, string channelName)
        {
            FileUtility.SaveFile(GetFilePath(channelName), csvData);
        }
        
        private string GetFilePath(string channelName)
        {
            string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{filePrefix}{formattedDateTime}{fileSuffix}";
            string filePath = Path.Combine(useStandardSaveLocation ? StandardSaveLocation : saveLocation, fileName);
            return $"{filePath}_{channelName}.csv";
        }
    }
}
