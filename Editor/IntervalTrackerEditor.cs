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

            for (int i = 0; i < intervalTracker.ChannelCount(); i++)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                _intervalChannel.Draw(intervalTracker.GetChannel(i));
            
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Add Channel"))
            {
                intervalTracker.channels.Add(new IntervalChannel());

                for (int i = 0; i < intervalTracker.ChannelCount(); i++)
                {
                    intervalTracker.GetChannel(i).ParentTracker = intervalTracker;
                    Debug.Log(i);
                    Debug.Log(intervalTracker.GetChannel(i).ParentTracker);
                    intervalTracker.GetChannel(i).ChannelIndex = i;
                }
            }
            
            // save changess
            if (GUI.changed)
            {
                EditorUtility.SetDirty(intervalTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
