using System.Collections.Generic;

namespace EasyLog.Core
{
    public class DataSet
    {
        private readonly List<DataPoint> _data = new List<DataPoint>();
        private readonly List<string> _measurementNames = new List<string>();

        public void Add(DataPoint dataPoint)
        {
            if (!_measurementNames.Contains(dataPoint.Name))
                _measurementNames.Add(dataPoint.Name);
        
            _data.Add(dataPoint);
        }

        public string SerializeForInflux()
        {
            string data = string.Empty;
        
            foreach (var dataPoint in _data)
            {
                data += $"{InfluxFormat(dataPoint.Name)} {InfluxFormat(dataPoint.Value)} {InfluxFormat(dataPoint.Time.ToString())}\n";
            }

            return data;
        }

        public string SerializeForCSV(char delimiter, char delimiterReplacement)
        {
            string data = string.Empty;
        
            foreach (var dataPoint in _data)
            {
                data += $"{dataPoint.Time.ToString().Replace(delimiter, delimiterReplacement)}" +
                        $"{delimiter}{dataPoint.Name.Replace(delimiter, delimiterReplacement)}" +
                        $"{delimiter}{dataPoint.Value.Replace(delimiter, delimiterReplacement)}\n";
            }

            return data;
        }

        private static string InfluxFormat(string input)
        {
            input = input.Replace(",", ".");
            return input.Replace(" ", "");
        }
    }
}
