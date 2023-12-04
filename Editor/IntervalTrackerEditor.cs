using System;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
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
            
            if (intervalTracker.ChannelCount() == 0)
                AddChannel(intervalTracker);

            if (intervalTracker.trackerMode == Tracker.TrackerMode.MultiChannel)
            {
                for (int i = 0; i < intervalTracker.ChannelCount(); i++)
                {
                    EditorGUI.indentLevel++;
                
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                    _intervalChannel.Draw(intervalTracker.GetChannel(i));

                    if (intervalTracker.ChannelCount() > 1)
                    {
                        if (GUILayout.Button("Remove Channel"))
                        {
                            intervalTracker.channels.Remove(intervalTracker.GetChannel(i));
                            UpdateChannels(intervalTracker);
                        }
                    }
                
                    EditorGUILayout.EndVertical();
                
                    EditorGUI.indentLevel--;
                }

                if (GUILayout.Button("Add Channel"))
                    AddChannel(intervalTracker);
            }
            else
            {
                _intervalChannel.Draw(intervalTracker.GetChannel(), true);
            }
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(intervalTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void AddChannel(IntervalTracker intervalTracker)
        {
            intervalTracker.channels.Add(new IntervalChannel());
            UpdateChannels(intervalTracker);
        }

        private void UpdateChannels(IntervalTracker intervalTracker)
        {
            for (int i = 0; i < intervalTracker.ChannelCount(); i++)
            {
                intervalTracker.GetChannel(i).ParentTracker = intervalTracker;
                intervalTracker.GetChannel(i).ChannelIndex = i;
            }
        }
    }
}
