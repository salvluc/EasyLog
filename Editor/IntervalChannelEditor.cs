using System;
using System.Linq;
using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    public class IntervalChannelEditor
    {
        private bool _showLogSettings = true;
        private readonly PropertySelectionEditor _propertySelection = new();

        public void Draw(IntervalChannel intervalChannel)
        {
            _showLogSettings = EditorGUILayout.Foldout(_showLogSettings, new GUIContent("Log Settings"), EditorStyles.foldoutHeader);

            if (_showLogSettings)
            {
                EditorGUI.indentLevel++;
                
                var trackOptions = Enum.GetValues(typeof(IntervalChannel.IntervalOption)).Cast<IntervalChannel.IntervalOption>().ToArray();
                int trackMethod = EditorGUILayout.Popup(
                    new GUIContent("Interval Type"),
                    (int)intervalChannel.intervalOption,
                    trackOptions.Select(e => e.ToString()).ToArray());
                intervalChannel.intervalOption = (IntervalChannel.IntervalOption)trackMethod;

                GUIContent logIntervalLabel = intervalChannel.intervalOption == IntervalChannel.IntervalOption.Seconds
                    ? new GUIContent("Log Every X Seconds", "How many seconds between the logs.")
                    : new GUIContent("Logs Per Second", "How many logs per second.");
            
                intervalChannel.logInterval = EditorGUILayout.IntSlider(
                    logIntervalLabel, intervalChannel.logInterval, 1, 60);
                
                var timeOptions = Enum.GetValues(typeof(Channel.TimeScaleOption)).Cast<Channel.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for logging and timestamps."),
                    (int)intervalChannel.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                intervalChannel.timeScaleOption = (Channel.TimeScaleOption)timeOption;

                intervalChannel.startAutomatically = EditorGUILayout.Toggle(
                    new GUIContent("Start Automatically", 
                        "True = Tracker starts logging on game start.\nFalse = Tracker has to be started manually."), 
                    intervalChannel.startAutomatically);

                if (!intervalChannel.startAutomatically)
                    EditorGUILayout.HelpBox("The tracker will be inactive until manually started with Unpause().", MessageType.Info);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            
            _propertySelection.DrawInterval(intervalChannel);
            
        }
    }
}