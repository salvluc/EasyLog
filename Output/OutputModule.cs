using System;

namespace EasyLog
{
    [Serializable]
    public abstract class OutputModule
    {
        public abstract string RequiredDataType { get; protected set; }

        public abstract void OnOutputRequested(string output);
    }
}
