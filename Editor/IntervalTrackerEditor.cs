using System;
using System.Linq;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(IntervalTracker))]
    public class IntervalTrackerEditor : UnityEditor.Editor
    {
        private bool _showFileSettings = true;
        private bool _showAdvancedFileOptions;
        private bool _showLogSettings = true;
        private bool _showTrackedProperties = true;

        private PropertySelectionEditor _propertySelection = new();
        private FileSettingsEditor _fileSettings = new();
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            IntervalTracker intervalTracker = (IntervalTracker)target;
            
            _fileSettings.Draw(intervalTracker);
            
            EditorGUILayout.Space();
            
            _showLogSettings = EditorGUILayout.Foldout(_showLogSettings, new GUIContent("Log Settings"), EditorStyles.foldoutHeader);

            if (_showLogSettings)
            {
                EditorGUI.indentLevel++;
                
                var trackOptions = Enum.GetValues(typeof(IntervalTracker.IntervalOption)).Cast<IntervalTracker.IntervalOption>().ToArray();
                int trackMethod = EditorGUILayout.Popup(
                    new GUIContent("Interval Type"),
                    (int)intervalTracker.intervalOption,
                    trackOptions.Select(e => e.ToString()).ToArray());
                intervalTracker.intervalOption = (IntervalTracker.IntervalOption)trackMethod;

                GUIContent logIntervalLabel = intervalTracker.intervalOption == IntervalTracker.IntervalOption.Seconds
                    ? new GUIContent("Log Every X Seconds", "How many seconds between the logs.")
                    : new GUIContent("Logs Per Second", "How many logs per second.");
            
                intervalTracker.logInterval = EditorGUILayout.IntSlider(
                    logIntervalLabel, intervalTracker.logInterval, 1, 60);
                
                var timeOptions = Enum.GetValues(typeof(IntervalTracker.TimeScaleOption)).Cast<IntervalTracker.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale"),
                    (int)intervalTracker.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                intervalTracker.timeScaleOption = (IntervalTracker.TimeScaleOption)timeOption;

                intervalTracker.startAutomatically = EditorGUILayout.Toggle(
                    new GUIContent("Start Automatically", 
                        "True = Tracker starts logging on game start.\nFalse = Tracker has to be started manually."), 
                    intervalTracker.startAutomatically);

                if (!intervalTracker.startAutomatically)
                    EditorGUILayout.HelpBox("The tracker will be inactive until manually started with Unpause().", MessageType.Info);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            
            _propertySelection.Draw(intervalTracker);
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(intervalTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}