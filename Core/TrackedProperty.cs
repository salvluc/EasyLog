using System;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public class TrackedProperty
    {
        public Component component;
        public string propertyName;
    }
}