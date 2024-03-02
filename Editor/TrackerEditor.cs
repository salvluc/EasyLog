using EasyLog.Core;
using EasyLog.Output;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(Tracker))]
    [CanEditMultipleObjects]
    public class TrackerEditor : UnityEditor.Editor
    {
        private readonly TrackerSettingsEditor _trackerSettingsEditor = new();
        private readonly OutputModuleEditor _outputModuleEditor = new();
        private readonly ChannelEditor _channelEditor = new();

        private bool _showOutputModules = true;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            Tracker tracker = (Tracker)target;

            _trackerSettingsEditor.Draw(tracker);
            
            EditorGUILayout.Space();
            
            _showOutputModules = EditorGUILayout.Foldout(_showOutputModules, "Output Modules", EditorStyles.foldoutHeader);
            
            if (tracker.outputModules.Count == 0)
                tracker.outputModules.Add(new InfluxWriter());

            if (_showOutputModules)
            {
                for (int i = 0; i < tracker.outputModules.Count; i++)
                {
                    EditorGUI.indentLevel++;
                
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                    _outputModuleEditor.Draw(tracker.outputModules[i]);

                    if (tracker.outputModules.Count > 1)
                    {
                        if (GUILayout.Button("Remove Module"))
                        {
                            tracker.outputModules.Remove(tracker.outputModules[i]);
                        }
                    }
                
                    EditorGUILayout.EndVertical();
                
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Add Influx Writer"))
                    tracker.outputModules.Add(new InfluxWriter());
                if (GUILayout.Button("Add Influx Uploader"))
                    tracker.outputModules.Add(new InfluxUploader());
                if (GUILayout.Button("Add CSV Writer"))
                    tracker.outputModules.Add(new CSVWriter());
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            if (tracker.ChannelCount == 0)
                AddChannel(tracker);

            if (tracker.trackerMode == Tracker.TrackerMode.MultiChannel)
            {
                for (int i = 0; i < tracker.ChannelCount; i++)
                {
                    EditorGUI.indentLevel++;
                
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                    _channelEditor.Draw(tracker.GetChannel(i));

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
                _channelEditor.Draw(tracker.GetChannel(), true);
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
