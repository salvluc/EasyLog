namespace EasyLog.Core
{
    public readonly struct DataPoint
    {
        public readonly string Name;
        public readonly string Time;
        public readonly string Value;

        public DataPoint(string name, string time, string value)
        {
            Name = name;
            Time = time;
            Value = value;
        }
    }
}
