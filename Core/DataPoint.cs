using System.Collections.Generic;

namespace EasyLog
{
    public readonly struct DataPoint
    {
        public readonly string MeasurementName;
        public readonly string Time;
        public readonly string ValueName;
        public readonly string Value;
        public readonly Dictionary<string, string> Tags;

        public DataPoint(string measurementName, string time, string valueName, string value, Dictionary<string, string> tags)
        {
            MeasurementName = measurementName;
            Time = time;
            ValueName = valueName;
            Value = value;
            Tags = tags;
        }
        
        public string SerializeForInflux()
        {
            return $"{FileUtility.InfluxFormat(MeasurementName)},{LogUtility.SerializeTags(Tags)}" +
                   $" {FileUtility.InfluxFormat(ValueName)}={FileUtility.InfluxValueFormat(Value)}" +
                   $" {FileUtility.InfluxFormat(Time)}";
        }
        
        public string SerializeForCsv(char delimiter, char delimiterReplacement)
        {
            return $"measurement={MeasurementName.Replace(delimiter, delimiterReplacement)},{LogUtility.SerializeTags(Tags)}," +
                   $"{ValueName.Replace(delimiter, delimiterReplacement)}={Value.Replace(delimiter, delimiterReplacement)}," +
                   $"{Time.Replace(delimiter, delimiterReplacement)}";
        }
    }
}
