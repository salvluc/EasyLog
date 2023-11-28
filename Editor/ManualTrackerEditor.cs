using System;
using System.Linq;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(ManualTracker))]
    public class ManualTrackerEditor : UnityEditor.Editor
    {
        private readonly TrackerSettingsEditor _trackerSettings = new();
        private readonly FileSettingsEditor _fileSettings = new();
        private readonly ManualChannelEditor _manualChannel = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ManualTracker manualTracker = (ManualTracker)target;
            
            _trackerSettings.Draw(manualTracker);
            
            EditorGUILayout.Space();
            
            _fileSettings.Draw(manualTracker);
            
            EditorGUILayout.Space();
            
            manualTracker.GetChannel().ParentTracker = manualTracker;
            
            _manualChannel.Draw(manualTracker.GetChannel());
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manualTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
