using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    public class ManualChannelEditor
    {
        private bool _showLogSettings = true;
        
        private readonly Dictionary<ManualChannel, bool> _channelFoldoutStates = new();
        private readonly Dictionary<ManualChannel, bool> _logSettingsFoldoutStates = new();
        private readonly PropertySelectionEditor _propertySelection = new();

        public void Draw(ManualChannel manualChannel, bool singleChannel = false)
        {
            _channelFoldoutStates.TryAdd(manualChannel, true); // default state

            _logSettingsFoldoutStates.TryAdd(manualChannel, true); // default state
            
            if (!singleChannel)
            {
                _channelFoldoutStates[manualChannel] = EditorGUILayout.Foldout(
                    _channelFoldoutStates[manualChannel],
                    "Channel " + manualChannel.ChannelIndex,
                    EditorStyles.foldoutHeader);


                if (!_channelFoldoutStates[manualChannel])
                    return;

                EditorGUI.indentLevel++;
            }
            
            _logSettingsFoldoutStates[manualChannel] = EditorGUILayout.Foldout(
                _logSettingsFoldoutStates[manualChannel], 
                "Log Settings", 
                EditorStyles.foldoutHeader);

            if (singleChannel)
                EditorGUI.indentLevel = 1;

            if (_logSettingsFoldoutStates[manualChannel])
            {
                var timeOptions = Enum.GetValues(typeof(Channel.TimeScaleOption)).Cast<Channel.TimeScaleOption>().ToArray();
                int timeOption = EditorGUILayout.Popup(
                    new GUIContent("Time Scale", "The time scale used for the log timestamps."),
                    (int)manualChannel.timeScaleOption,
                    timeOptions.Select(e => e.ToString()).ToArray());
                manualChannel.timeScaleOption = (Channel.TimeScaleOption)timeOption;
                
                manualChannel.logOnStart = EditorGUILayout.Toggle(
                    new GUIContent("Log on Start", "If true, the tracker will log the values once when the game starts."), 
                    manualChannel.logOnStart);
            }

            EditorGUILayout.Space();
            
            if (singleChannel)
            {
                EditorGUI.indentLevel = 0;
                _propertySelection.DrawManual(manualChannel);
            }
            else
            {
                _propertySelection.DrawManual(manualChannel);
                EditorGUI.indentLevel--;
            }
        }
    }
}
