using UnityEngine;

namespace EasyLog.Editor
{
    public static class StyleKit
    {
        public static Color InfluxColor = new (2.7f, 0, 1.6f, 0.3f);
        public static Color CsvColor = new (1.2f, 3f, 0, 0.15f);
        public static Color RemoveColor = new (2.7f, 0.9f, 0.81f, 0.3f);
        
        public static char StringToChar(string inputString)
        {
            inputString = inputString[..1];
            return inputString[0];
        }
    }
}
