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
        private readonly OutputSettingsEditor _outputSettings = new();
        private readonly ManualChannelEditor _manualChannel = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ManualTracker manualTracker = (ManualTracker)target;
            
            _trackerSettings.Draw(manualTracker);
            
            EditorGUILayout.Space();
            
            _outputSettings.Draw(manualTracker);
            
            EditorGUILayout.Space();
            
            if (manualTracker.ChannelCount() == 0)
                AddChannel(manualTracker);
            
            if (manualTracker.trackerMode == Tracker.TrackerMode.MultiChannel)
            {
                for (int i = 0; i < manualTracker.ChannelCount(); i++)
                {
                    EditorGUI.indentLevel++;
                
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                    _manualChannel.Draw(manualTracker.GetChannel(i));

                    if (manualTracker.ChannelCount() > 1)
                    {
                        if (GUILayout.Button("Remove Channel"))
                        {
                            manualTracker.channels.Remove(manualTracker.GetChannel(i));
                            UpdateChannels(manualTracker);
                        }
                    }
                
                    EditorGUILayout.EndVertical();
                
                    EditorGUI.indentLevel--;
                }

                if (GUILayout.Button("Add Channel"))
                    AddChannel(manualTracker);
            }
            else
            {
                _manualChannel.Draw(manualTracker.GetChannel(), true);
            }
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manualTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void AddChannel(ManualTracker manualTracker)
        {
            manualTracker.channels.Add(new ManualChannel());
            UpdateChannels(manualTracker);
        }

        private void UpdateChannels(ManualTracker manualTracker)
        {
            for (int i = 0; i < manualTracker.ChannelCount(); i++)
            {
                manualTracker.GetChannel(i).ParentTracker = manualTracker;
                manualTracker.GetChannel(i).ChannelIndex = i;
            }
        }
    }
}
