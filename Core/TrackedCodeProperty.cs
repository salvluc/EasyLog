using System;

namespace EasyLog
{
    public class TrackedCodeProperty
    {
        public Func<object> Accessor;
        public string Name;
    }
}
