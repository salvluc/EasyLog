using System;
using System.Linq;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class OutputSettingsEditor
    {
        private bool _showFileSettings = true;
        private bool _showAdvancedFileOptions;
        
        public void Draw(Tracker tracker)
        {
            _showFileSettings = EditorGUILayout.Foldout(_showFileSettings, new GUIContent("Output Settings"), EditorStyles.foldoutHeader);

            if (!_showFileSettings) return;
            
            EditorGUI.indentLevel++;
                
            var outputModes = Enum.GetValues(typeof(Tracker.OutputFormat)).Cast<Tracker.OutputFormat>().ToArray();
            int outputMode = EditorGUILayout.Popup(
                new GUIContent("Output Format"),
                (int)tracker.outputFormat,
                outputModes.Select(e => e.ToString()).ToArray());
            tracker.outputFormat = (Tracker.OutputFormat)outputMode;
            
            tracker.filePrefix = EditorGUILayout.TextField(
                new GUIContent("File Prefix",
                    "The prefix of the saved file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
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
                    "The suffix of the saved .csv file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
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
                    new GUIContent("Delimiter", "Delimiter, only affects .csv output."),
                    tracker.delimiter.ToString());

                if (!string.IsNullOrEmpty(delimiter))
                    tracker.delimiter = TrackerEditorUtility.StringToChar(delimiter);
                    
                string delimiterReplacement = EditorGUILayout.TextField(
                    new GUIContent("Delimiter Replacement", "If the delimiter is a part of a value," +
                                                            " it will be replaced with this."),
                    tracker.delimiterReplacement.ToString());
                    
                if (!string.IsNullOrEmpty(delimiterReplacement))
                    tracker.delimiterReplacement = TrackerEditorUtility.StringToChar(delimiterReplacement);
                    
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
    }
}
