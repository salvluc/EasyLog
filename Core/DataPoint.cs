namespace EasyLog.Core
{
    public readonly struct DataPoint
    {
        public readonly string Name;
        public readonly float Time;
        public readonly string Value;

        public DataPoint(string name, float time, string value)
        {
            Name = name;
            Time = time;
            Value = value;
        }
    }
}
