using System;
using System.Linq;
using EasyLog.Core;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    public class TrackerSettingsEditor
    {
        private bool _showSettings = true;
        
        public void Draw(Tracker tracker)
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, new GUIContent("Tracker Settings"), EditorStyles.foldoutHeader);

            if (_showSettings)
            {
                EditorGUI.indentLevel++;
                
                var trackModes = Enum.GetValues(typeof(Tracker.TrackerMode)).Cast<Tracker.TrackerMode>().ToArray();
                int trackMode = EditorGUILayout.Popup(
                    new GUIContent("Tracker Mode"),
                    (int)tracker.trackerMode,
                    trackModes.Select(e => e.ToString()).ToArray());
                tracker.trackerMode = (Tracker.TrackerMode)trackMode;
                
                EditorGUI.indentLevel--;
            }
        }
    }
}
