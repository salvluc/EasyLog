using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Draw(Channel channel, bool singleChannel = false)
        {
            _channelFoldoutStates.TryAdd(channel, true); // default state

            _logSettingsFoldoutStates.TryAdd(channel, true); // default state

            if (!singleChannel)
            {
                _channelFoldoutStates[channel] = EditorGUILayout.Foldout(
                    _channelFoldoutStates[channel],
                    "Channel " + channel.ChannelIndex,
                    EditorStyles.foldoutHeader);
                
                if (!_channelFoldoutStates[channel])
                    return;

                EditorGUI.indentLevel++;
            }

            _logSettingsFoldoutStates[channel] = EditorGUILayout.Foldout(_logSettingsFoldoutStates[channel], "Log Settings", EditorStyles.foldoutHeader);

            if (singleChannel)
                EditorGUI.indentLevel = 1;

            if (_logSettingsFoldoutStates[channel])
            {
                var trackOptions = Enum.GetValues(typeof(Channel.IntervalOption)).Cast<Channel.IntervalOption>().ToArray();
                int trackMethod = EditorGUILayout.Popup(
                    new GUIContent("Interval Type"),
                    (int)channel.intervalOption,
                    trackOptions.Select(e => e.ToString()).ToArray());
                channel.intervalOption = (Channel.IntervalOption)trackMethod;

                GUIContent logIntervalLabel = channel.intervalOption == Channel.IntervalOption.Seconds
                    ? new GUIContent("Log Every X Seconds", "How many seconds between the logs.")
                    : new GUIContent("Logs Per Second", "How many logs per second.");
            
                channel.logInterval = EditorGUILayout.IntSlider(logIntervalLabel, channel.logInterval, 0, 60);
                
                if (channel.logInterval == 0)
                    EditorGUILayout.HelpBox("By setting this to 0, values will not be tracked automatically.", MessageType.Info);
                
                var timeOptions = Enum.GetValues(typeof(Channel.TimeScaleOption)).Cast<Channel.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for logging and timestamps."),
                    (int)channel.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                channel.timeScaleOption = (Channel.TimeScaleOption)timeOption;
            }

            EditorGUILayout.Space();

            if (singleChannel)
            {
                EditorGUI.indentLevel = 0;
                _propertySelection.Draw(channel);
            }
            else
            {
                _propertySelection.Draw(channel);
                EditorGUI.indentLevel--;
            }
        }
    }
}