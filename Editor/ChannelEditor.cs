using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class ChannelEditor
    {
        private readonly Dictionary<Channel, bool> _channelFoldoutStates = new();
        private readonly Dictionary<Channel, bool> _logSettingsFoldoutStates = new();
        private readonly PropertySelectionEditor _propertySelection = new();

        public void Draw(Channel Channel, bool singleChannel = false)
        {
            _channelFoldoutStates.TryAdd(Channel, true); // default state

            _logSettingsFoldoutStates.TryAdd(Channel, true); // default state

            if (!singleChannel)
            {
                _channelFoldoutStates[Channel] = EditorGUILayout.Foldout(
                    _channelFoldoutStates[Channel],
                    "Channel " + Channel.ChannelIndex,
                    EditorStyles.foldoutHeader);


                if (!_channelFoldoutStates[Channel])
                    return;

                EditorGUI.indentLevel++;
            }

            _logSettingsFoldoutStates[Channel] = EditorGUILayout.Foldout(
                _logSettingsFoldoutStates[Channel], 
                "Log Settings", 
                EditorStyles.foldoutHeader);

            if (singleChannel)
                EditorGUI.indentLevel = 1;

            if (_logSettingsFoldoutStates[Channel])
            {
                var trackOptions = Enum.GetValues(typeof(Channel.IntervalOption)).Cast<Channel.IntervalOption>().ToArray();
                int trackMethod = EditorGUILayout.Popup(
                    new GUIContent("Interval Type"),
                    (int)Channel.intervalOption,
                    trackOptions.Select(e => e.ToString()).ToArray());
                Channel.intervalOption = (Channel.IntervalOption)trackMethod;

                GUIContent logIntervalLabel = Channel.intervalOption == Channel.IntervalOption.Seconds
                    ? new GUIContent("Log Every X Seconds", "How many seconds between the logs.")
                    : new GUIContent("Logs Per Second", "How many logs per second.");
            
                Channel.logInterval = EditorGUILayout.IntSlider(
                    logIntervalLabel, Channel.logInterval, 0, 60);
                
                if (Channel.logInterval == 0)
                    EditorGUILayout.HelpBox("By setting this to 0, values will not be tracked automatically.", MessageType.Info);
                
                var timeOptions = Enum.GetValues(typeof(Channel.TimeScaleOption)).Cast<Channel.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for logging and timestamps."),
                    (int)Channel.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                Channel.timeScaleOption = (Channel.TimeScaleOption)timeOption;
            }

            EditorGUILayout.Space();

            if (singleChannel)
            {
                EditorGUI.indentLevel = 0;
                _propertySelection.DrawInterval(Channel);
            }
            else
            {
                _propertySelection.DrawInterval(Channel);
                EditorGUI.indentLevel--;
            }
        }
    }
}