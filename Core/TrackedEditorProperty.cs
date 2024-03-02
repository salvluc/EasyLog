using System;
using UnityEngine;

namespace EasyLog
{
    [Serializable]
    public class TrackedEditorProperty
    {
        public Component component;
        public string propertyName;
        public string Name => $"{component.gameObject.name}{component.GetType().Name}{propertyName}";
    }
}