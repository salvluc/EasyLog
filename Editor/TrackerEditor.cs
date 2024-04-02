using UnityEditor;
using System.Collections.Generic;
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

        private readonly Dictionary<OutputModule, bool> _outputModuleFoldoutStates = new();
        private bool _showOutputModules = true;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            Tracker tracker = (Tracker)target;
            
            Color colorCache = GUI.backgroundColor;

            _trackerSettingsEditor.Draw(tracker);
            
            EditorGUILayout.Space();
            
            _showOutputModules = EditorGUILayout.Foldout(_showOutputModules, "Output Modules", EditorStyles.foldoutHeader);
            
            if (tracker.outputModules.Count == 0)
                tracker.outputModules.Add(new InfluxWriter());

            if (_showOutputModules)
            {
                for (int i = 0; i < tracker.outputModules.Count; i++)
                {
                    EditorGUILayout.Space(2);
                    
                    var outputModule = tracker.outputModules[i];
                    
                    _outputModuleFoldoutStates.TryAdd(outputModule, true);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    
                    // ensure the foldout title is on the left
                    //_outputModuleFoldoutStates[outputModule] = EditorGUILayout.Foldout(_outputModuleFoldoutStates[outputModule], outputModule.GetType().Name);
                    EditorGUILayout.LabelField(outputModule.GetType().Name, EditorStyles.boldLabel);
                    
                    GUILayout.FlexibleSpace();
                    if (tracker.outputModules.Count > 1)
                    {
                        GUI.backgroundColor = StyleKit.RemoveColor;
                        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            _outputModuleFoldoutStates.Remove(outputModule);
                            tracker.outputModules.Remove(outputModule);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            continue;
                        }
                        GUI.backgroundColor = colorCache;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space(4);
                    
                    if (_outputModuleFoldoutStates[outputModule])
                    {
                        EditorGUI.indentLevel++;
                        _outputModuleEditor.Draw(outputModule);
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space(1);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space(2);
                
                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = StyleKit.CsvColor;
                if (GUILayout.Button("Add Influx Writer"))
                    tracker.outputModules.Add(new InfluxWriter());
                if (GUILayout.Button("Add Influx Uploader"))
                    tracker.outputModules.Add(new InfluxUploader());
                if (GUILayout.Button("Add CSV Writer"))
                    tracker.outputModules.Add(new CSVWriter());
                if (GUILayout.Button("Add System Info Writer"))
                    tracker.outputModules.Add(new SystemInfoWriter());
                GUI.backgroundColor = colorCache;
                
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
                        //GUI.backgroundColor = StyleKit.RemoveColor;
                        if (GUILayout.Button("Remove Channel"))
                        {
                            tracker.channels.Remove(tracker.GetChannel(i));
                            UpdateChannels(tracker);
                        }
                        //GUI.backgroundColor = colorCache;
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
                tracker.GetChannel(i).tracker = tracker;
                tracker.GetChannel(i).channelIndex = i;
            }
        }
    }
}
