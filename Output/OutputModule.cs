using System;

namespace EasyLog.Output
{
    [Serializable]
    public abstract class OutputModule
    {
        public abstract string RequiredDataType { get; protected set; }

        public abstract void OnOutputRequested(string output);
    }
}
