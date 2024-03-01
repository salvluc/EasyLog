using System;

namespace EasyLog.Core
{
    public class TrackedCodeProperty
    {
        public Func<object> Accessor;
        public string Name;
    }
}
