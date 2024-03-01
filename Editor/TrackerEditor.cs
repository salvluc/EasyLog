using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(Tracker))]
    [CanEditMultipleObjects]
    public class TrackerEditor : UnityEditor.Editor
    {
        private readonly TrackerSettingsEditor _trackerSettings = new();
        private readonly OutputSettingsEditor _outputSettings = new();
        private readonly ChannelEditor _channel = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            Tracker tracker = (Tracker)target;

            _trackerSettings.Draw(tracker);
            
            EditorGUILayout.Space();
            
            _outputSettings.Draw(tracker);
            
            EditorGUILayout.Space();
            
            if (tracker.ChannelCount == 0)
                AddChannel(tracker);

            if (tracker.trackerMode == Tracker.TrackerMode.MultiChannel)
            {
                for (int i = 0; i < tracker.ChannelCount; i++)
                {
                    EditorGUI.indentLevel++;
                
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                    _channel.Draw(tracker.GetChannel(i));

                    if (tracker.ChannelCount > 1)
                    {
                        if (GUILayout.Button("Remove Channel"))
                        {
                            tracker.channels.Remove(tracker.GetChannel(i));
                            UpdateChannels(tracker);
                        }
                    }
                
                    EditorGUILayout.EndVertical();
                
                    EditorGUI.indentLevel--;
                }

                if (GUILayout.Button("Add Channel"))
                    AddChannel(tracker);
            }
            else
            {
                _channel.Draw(tracker.GetChannel(), true);
            }
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(tracker);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void AddChannel(Tracker tracker)
        {
            tracker.channels.Add(new Channel());
            UpdateChannels(tracker);
        }

        private void UpdateChannels(Tracker tracker)
        {
            for (int i = 0; i < tracker.ChannelCount; i++)
            {
                tracker.GetChannel(i).Tracker = tracker;
                tracker.GetChannel(i).ChannelIndex = i;
            }
        }
    }
}
