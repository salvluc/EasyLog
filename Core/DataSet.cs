using System.Collections.Generic;

namespace EasyLog.Core
{
    public class DataSet
    {
        private readonly List<DataPoint> _data = new List<DataPoint>();
        private readonly List<string> _measurementNames = new List<string>();

        public void Add(DataPoint dataPoint)
        {
            if (!_measurementNames.Contains(dataPoint.MeasurementName))
                _measurementNames.Add(dataPoint.MeasurementName);
        
            _data.Add(dataPoint);
        }

        public string SerializeForInflux()
        {
            string data = string.Empty;
        
            foreach (var dataPoint in _data)
            {
                data += $"{dataPoint.SerializeForInflux()}\n";
            }

            return data;
        }

        public string SerializeForCSV(char delimiter, char delimiterReplacement)
        {
            string data = string.Empty;
        
            foreach (var dataPoint in _data)
            {
                data += $"{dataPoint.Time.ToString().Replace(delimiter, delimiterReplacement)}" +
                        $"{delimiter}{dataPoint.MeasurementName.Replace(delimiter, delimiterReplacement)}" +
                        $"{delimiter}{dataPoint.Value.Replace(delimiter, delimiterReplacement)}\n";
            }

            return data;
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
    }
}
