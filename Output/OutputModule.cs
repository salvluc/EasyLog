using System;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public abstract class OutputModule
    {
        public bool useStandardSaveLocation;

        protected string StandardSaveLocation = Application.dataPath + "/EasyLog";

        public abstract void OnOutputRequested(string output, string channelName);
    }
}
