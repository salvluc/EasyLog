using UnityEngine;

namespace EasyLog.Editor
{
    public static class StyleKit
    {
        public static Color InfluxColor = new (1.9f, 0, 1.08f, 0.3f);
        public static Color CsvColor = new (0.81f, 2.1f, 0, 0.15f);
        public static Color RemoveColor = new (1.88f, 0.6f, 0.54f, 0.3f);
        
        public static char StringToChar(string inputString)
        {
            inputString = inputString[..1];
            return inputString[0];
        }
    }
}
