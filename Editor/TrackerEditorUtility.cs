using UnityEngine;

namespace EasyLog.Editor
{
    public static class TrackerEditorUtility
    {
        public static char StringToChar(string inputString)
        {
            inputString = inputString[..1];
            return inputString[0];
        }
    }
}
