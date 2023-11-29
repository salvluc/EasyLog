using System;
using System.Linq;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    public class ManualChannelEditor
    {
        private bool _showLogSettings = true;
        
        private readonly PropertySelectionEditor _propertySelection = new();

        public void Draw(ManualChannel manualChannel)
        {
            _showLogSettings = EditorGUILayout.Foldout(_showLogSettings, new GUIContent("Log Settings"), EditorStyles.foldoutHeader);

            if (_showLogSettings)
            {
                EditorGUI.indentLevel++;
                
                var timeOptions = Enum.GetValues(typeof(Channel.TimeScaleOption)).Cast<Channel.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for the log timestamps."),
                    (int)manualChannel.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                manualChannel.timeScaleOption = (Channel.TimeScaleOption)timeOption;
                
                manualChannel.logOnStart = EditorGUILayout.Toggle(
                    new GUIContent("Log on Start", "If true, the tracker will log the values once when the game starts."), 
                    manualChannel.logOnStart);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            
            //_propertySelection.DrawManual((ManualTracker)manualChannel.ParentTracker);
        }
    }
}
