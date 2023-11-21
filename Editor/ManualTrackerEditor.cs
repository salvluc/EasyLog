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
        private bool _showAdvancedFileOptions;
        private bool _showLogSettings = true;

        private readonly PropertySelectionEditor _propertySelection = new();
        private readonly FileSettingsEditor _fileSettings = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ManualTracker manualTracker = (ManualTracker)target;
            
            _fileSettings.Draw(manualTracker);
            
            EditorGUILayout.Space();
            
            _showLogSettings = EditorGUILayout.Foldout(_showLogSettings, new GUIContent("Log Settings"), EditorStyles.foldoutHeader);

            if (_showLogSettings)
            {
                EditorGUI.indentLevel++;
                
                var timeOptions = Enum.GetValues(typeof(Tracker.TimeScaleOption)).Cast<Tracker.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for the log timestamps."),
                    (int)manualTracker.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                manualTracker.timeScaleOption = (Tracker.TimeScaleOption)timeOption;
                
                manualTracker.logOnStart = EditorGUILayout.Toggle(
                    new GUIContent("Log on Start", "If true, the tracker will log the values once when the game starts."), 
                    manualTracker.logOnStart);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            
            _propertySelection.Draw(manualTracker);
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manualTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
