using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class TrackerSettingsEditor
    {
        private bool _showSettings = true;
        
        public void Draw(Tracker tracker)
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, new GUIContent("Tracker Settings"), EditorStyles.foldoutHeader);

            if (!_showSettings) return;
            
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
