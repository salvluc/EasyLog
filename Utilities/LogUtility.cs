using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyLog
{
    public static class LogUtility
    {
        public static string GetSystemInfo()
        {
            string info = "Operating System: " + SystemInfo.operatingSystem + "\n";
            info += "CPU: " + SystemInfo.processorType + " (" + SystemInfo.processorCount + " cores)\n";
            info += "RAM: " + SystemInfo.systemMemorySize + "MB\n";
            info += "GPU: " + SystemInfo.graphicsDeviceName + "\n";
            info += "VRAM: " + SystemInfo.graphicsMemorySize + "MB\n";

            return info;
        }
        
        public static Dictionary<string, string> GetSystemInfoAsTags()
        {
            Dictionary<string, string> info = new Dictionary<string, string>
            {
                ["operatingSystem"] = FileUtility.InfluxFormat(SystemInfo.operatingSystem),
                ["cpu"] = FileUtility.InfluxFormat(SystemInfo.processorType) + "(" + SystemInfo.processorCount + "cores)",
                ["ram"] = FileUtility.InfluxFormat(SystemInfo.systemMemorySize + "MB"),
                ["gpu"] = FileUtility.InfluxFormat(SystemInfo.graphicsDeviceName),
                ["vram"] = FileUtility.InfluxFormat(SystemInfo.graphicsMemorySize + "MB")
            };

            return info;
        }
        
        public static Dictionary<string, string> GetSystemInfoAsFields()
        {
            Dictionary<string, string> info = new Dictionary<string, string>
            {
                ["operatingSystem"] = "\"" + FileUtility.InfluxFormat(SystemInfo.operatingSystem) + "\"",
                ["cpu"] = "\"" + FileUtility.InfluxFormat(SystemInfo.processorType) + "(" + SystemInfo.processorCount + "cores)" + "\"",
                ["ram"] = "\"" + FileUtility.InfluxFormat(SystemInfo.systemMemorySize + "MB") + "\"",
                ["gpu"] = "\"" + FileUtility.InfluxFormat(SystemInfo.graphicsDeviceName) + "\"",
                ["vram"] = "\"" + FileUtility.InfluxFormat(SystemInfo.graphicsMemorySize + "MB" + "\"")
            };

            return info;
        }
        
        public static string SerializeTags(Dictionary<string,string> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            IEnumerable<string> items = from tags in dictionary
                select tags.Key + "=" + tags.Value;

            return string.Join(",", items);
        }
    }
}
