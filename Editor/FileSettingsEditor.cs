using System;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class FileSettingsEditor
    {
        private bool _showFileSettings = true;
        private bool _showAdvancedFileOptions;
        
        public void Draw(Tracker tracker)
        {
            _showFileSettings = EditorGUILayout.Foldout(_showFileSettings, new GUIContent("File Settings"), EditorStyles.foldoutHeader);

            if (_showFileSettings)
            {
                EditorGUI.indentLevel++;
                
                tracker.filePrefix = EditorGUILayout.TextField(
                    new GUIContent("File Prefix",
                        "The prefix of the saved .csv file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix.csv"),
                    tracker.filePrefix);

                if (string.IsNullOrEmpty(tracker.fileSuffix))
                {
                    if (tracker is IntervalTracker)
                        tracker.fileSuffix = "Interval";
                    if (tracker is ManualTracker)
                        tracker.fileSuffix = "Manual";
                }
                
                tracker.fileSuffix = EditorGUILayout.TextField(
                    new GUIContent("File Suffix",
                        "The suffix of the saved .csv file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix.csv"),
                    tracker.fileSuffix);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Save Location", tracker.saveLocation);

                if (GUILayout.Button("Open",GUILayout.Width(45)))
                {
                    Application.OpenURL(tracker.saveLocation);
                }
                
                if (GUILayout.Button("Change", GUILayout.Width(55)))
                {
                    var newSaveLocation = EditorUtility.OpenFolderPanel("Select Save Location", tracker.saveLocation, "");
                    tracker.saveLocation = newSaveLocation == "" ? tracker.saveLocation : newSaveLocation;
                }
                
                EditorGUILayout.EndHorizontal();

                _showAdvancedFileOptions = EditorGUILayout.Foldout(_showAdvancedFileOptions, "Advanced");

                if (_showAdvancedFileOptions)
                {
                    EditorGUI.indentLevel++;
                    
                    string delimiter = EditorGUILayout.TextField(
                        new GUIContent("Delimiter", "The delimiter that separates the columns in the .csv file."),
                        tracker.delimiter.ToString());

                    if (!string.IsNullOrEmpty(delimiter))
                        tracker.delimiter = TrackerEditorUtility.StringToChar(delimiter);
                    
                    string delimiterReplacement = EditorGUILayout.TextField(
                        new GUIContent("Delimiter Replacement", "If the delimiter is a part of a logged value," +
                                                                " it will be replaced with this to protect column separation."),
                        tracker.delimiterReplacement.ToString());
                    
                    if (!string.IsNullOrEmpty(delimiterReplacement))
                        tracker.delimiterReplacement = TrackerEditorUtility.StringToChar(delimiterReplacement);
                    
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
