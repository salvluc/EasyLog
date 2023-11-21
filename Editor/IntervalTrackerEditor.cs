using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            IntervalTracker intervalTracker = (IntervalTracker)target;
            
            _showFileSettings = EditorGUILayout.Foldout(_showFileSettings, new GUIContent("File Settings"), EditorStyles.foldoutHeader);

            if (_showFileSettings)
            {
                EditorGUI.indentLevel++;
                
                intervalTracker.filePrefix = EditorGUILayout.TextField(
                    new GUIContent("File Prefix",
                        "The prefix of the saved .csv file. The full file name will be: Prefix_dd-MM-yyyy_HH-mm.csv"),
                    intervalTracker.filePrefix);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Save Location", intervalTracker.saveLocation);

                if (GUILayout.Button("Open",GUILayout.Width(45)))
                {
                    Application.OpenURL(intervalTracker.saveLocation);
                }
                
                if (GUILayout.Button("Change", GUILayout.Width(55)))
                {
                    var newSaveLocation = EditorUtility.OpenFolderPanel("Select Save Location", intervalTracker.saveLocation, "");
                    intervalTracker.saveLocation = newSaveLocation == "" ? intervalTracker.saveLocation : newSaveLocation;
                }
                
                EditorGUILayout.EndHorizontal();

                _showAdvancedFileOptions = EditorGUILayout.Foldout(_showAdvancedFileOptions, "Advanced");

                if (_showAdvancedFileOptions)
                {
                    EditorGUI.indentLevel++;
                    
                    string delimiter = EditorGUILayout.TextField(
                        new GUIContent("Delimiter", "The delimiter that separates the columns in the .csv file."),
                        intervalTracker.delimiter.ToString());

                    if (!string.IsNullOrEmpty(delimiter))
                        intervalTracker.delimiter = TrackerEditorUtility.StringToChar(delimiter);
                    
                    string delimiterReplacement = EditorGUILayout.TextField(
                        new GUIContent("Delimiter Replacement", "If the delimiter is a part of a logged value," +
                                                                " it will be replaced with this to protect column separation."),
                        intervalTracker.delimiterReplacement.ToString());
                    
                    if (!string.IsNullOrEmpty(delimiterReplacement))
                        intervalTracker.delimiterReplacement = TrackerEditorUtility.StringToChar(delimiterReplacement);
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            
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
            
            _showTrackedProperties = EditorGUILayout.Foldout(_showTrackedProperties, new GUIContent("Tracked Properties"), EditorStyles.foldoutHeader);

            if (_showTrackedProperties)
            {
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < intervalTracker.trackedPropertiesViaEditor.Count; i++)
                {
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.BeginHorizontal();

                    // GAMEOBJECT SELECTION
                    GameObject selectedObject = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        (intervalTracker.trackedPropertiesViaEditor[i].component)?.gameObject,
                        typeof(GameObject),
                        true
                    ) as GameObject;

                    // COMPONENT DROPDOWN
                    if (selectedObject != null)
                    {
                        // if the GameObject has changed, reset the component and property
                        if ((intervalTracker.trackedPropertiesViaEditor[i].component)?.gameObject != selectedObject)
                        {
                            // pick transform component as default (might throw exceptions)
                            intervalTracker.trackedPropertiesViaEditor[i].component = selectedObject.GetComponent<Transform>();
                            intervalTracker.trackedPropertiesViaEditor[i].propertyName = null;
                        }

                        // get all components on the GameObject
                        Component[] components = selectedObject.GetComponents<Component>();
                        List<string> componentNames = components.Select(comp => comp.GetType().Name).ToList();
                        int currentComponentIndex = components.ToList().IndexOf(intervalTracker.trackedPropertiesViaEditor[i].component);
                        int newComponentIndex = EditorGUILayout.Popup(currentComponentIndex, componentNames.ToArray());

                        // update the selected component
                        if (newComponentIndex >= 0)
                        {
                            intervalTracker.trackedPropertiesViaEditor[i].component = components[newComponentIndex];
                        }
                    }

                    // PROPERTY DROPDOWN
                    if (intervalTracker.trackedPropertiesViaEditor[i].component != null)
                    {
                        // get list of properties
                        var properties = intervalTracker.trackedPropertiesViaEditor[i].component.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Select(p => p.Name)
                            .ToList();

                        // get list of fields
                        var fields = intervalTracker.trackedPropertiesViaEditor[i].component.GetType()
                            .GetFields(BindingFlags.Instance | BindingFlags.Public)
                            .Select(f => f.Name)
                            .ToList();

                        // combine lists
                        var propertiesAndFields = properties.Concat(fields).ToList();

                        // find index of currently selected property
                        int currentIndex = propertiesAndFields.IndexOf(intervalTracker.trackedPropertiesViaEditor[i].propertyName);
                        if (currentIndex == -1) currentIndex = 0;

                        // select new property
                        int newIndex = EditorGUILayout.Popup(currentIndex, propertiesAndFields.ToArray());
                        intervalTracker.trackedPropertiesViaEditor[i].propertyName = propertiesAndFields[newIndex];
                    }

                    // REMOVE BUTTON
                    if (GUILayout.Button("Remove"))
                    {
                        intervalTracker.trackedPropertiesViaEditor.RemoveAt(i);
                    }

                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Property"))
                {
                    intervalTracker.trackedPropertiesViaEditor.Add(new TrackedProperty());
                }

                EditorGUI.indentLevel--;
            }
            
            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(intervalTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}