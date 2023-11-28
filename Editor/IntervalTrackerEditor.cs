using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(IntervalTracker))]
    [CanEditMultipleObjects]
    public class IntervalTrackerEditor : UnityEditor.Editor
    {
        private readonly TrackerSettingsEditor _trackerSettings = new();
        private readonly FileSettingsEditor _fileSettings = new();
        private readonly IntervalChannelEditor _intervalChannel = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            IntervalTracker intervalTracker = (IntervalTracker)target;
            
            _trackerSettings.Draw(intervalTracker);
            
            EditorGUILayout.Space();
            
            _fileSettings.Draw(intervalTracker);
            
            EditorGUILayout.Space();

            intervalTracker.GetChannel().ParentTracker = intervalTracker;
            
            _intervalChannel.Draw(intervalTracker.GetChannel());
            
            // save changess
            if (GUI.changed)
            {
                EditorUtility.SetDirty(intervalTracker);
                serializedObject.ApplyModifiedProperties();
                Debug.Log(intervalTracker.GetChannel().trackedPropertiesViaEditor[0].propertyName);
            }
        }
    }
}
