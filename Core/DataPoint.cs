using System;
using System.Collections.Generic;
using System.Linq;

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
            return $"{InfluxFormat(MeasurementName)},{SerializeTags(Tags)}" +
                   $" {InfluxFormat(ValueName)}={InfluxValueFormat(Value)}" +
                   $" {InfluxFormat(Time)}";
        }
        
        public string SerializeForCsv(char delimiter, char delimiterReplacement)
        {
            return $"measurement={MeasurementName.Replace(delimiter, delimiterReplacement)},{SerializeTags(Tags)}," +
                   $"{ValueName.Replace(delimiter, delimiterReplacement)}={Value.Replace(delimiter, delimiterReplacement)}," +
                   $"{Time.Replace(delimiter, delimiterReplacement)}";
        }
        
        private static string InfluxFormat(string input)
        {
            input = input.Replace(",", ".");
            return input.Replace(" ", "");
        }
        
        private static string InfluxValueFormat(string input)
        {
            if (float.TryParse(input, out float floatParse))
                return InfluxFormat(input);
            
            return "\"" + InfluxFormat(input) + "\"";
        }
        
        private static string SerializeTags(Dictionary<string,string> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            IEnumerable<string> items = from tags in dictionary
                select tags.Key + "=" + tags.Value;

            return string.Join(",", items);
        }
    }
}
