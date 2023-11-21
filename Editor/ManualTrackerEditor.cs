using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyLog.Trackers;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [CustomEditor(typeof(ManualTracker))]
    public class ManualTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ManualTracker manualTracker = (ManualTracker)target;

            EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);

            EditorGUILayout.TextField(
                new GUIContent("File Prefix",
                    "The prefix of the saved .csv file. The full file name will be: Prefix_dd-MM-yyyy_HH-mm.csv"),
                manualTracker.filePrefix);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Save Location:", manualTracker.saveLocation);

            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                Application.OpenURL(manualTracker.saveLocation);
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Change Save Location"))
            {
                var newSaveLocation =
                    EditorUtility.OpenFolderPanel("Select Save Location", manualTracker.saveLocation, "");
                manualTracker.saveLocation = newSaveLocation == "" ? manualTracker.saveLocation : newSaveLocation;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Settings", EditorStyles.boldLabel);

            // TRACK OPTIONS
            /*var trackOptions = Enum.GetValues(typeof(SimpleTracker.TrackOption)).Cast<SimpleTracker.TrackOption>().ToArray();
            int trackMethod = EditorGUILayout.Popup(
                new GUIContent("Track by"),
                (int)simpleTracker.trackMethod,
                trackOptions.Select(e => e.ToString()).ToArray());
            simpleTracker.trackMethod = (SimpleTracker.TrackOption)trackMethod;*/

            if (manualTracker.trackMethod == ManualTracker.TrackOption.Interval)
            {
                manualTracker.logsPerSecond = EditorGUILayout.IntSlider(
                    new GUIContent("Logs per second", "How often the values should be saved per second."),
                    manualTracker.logsPerSecond, 0, 60);
            }

            if (manualTracker.logsPerSecond == 0f)
            {
                EditorGUILayout.HelpBox(
                    "By setting this to 0, values will be only saved when manually calling LogEvent().",
                    MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tracked Properties", EditorStyles.boldLabel);

            for (int i = 0; i < manualTracker.trackedPropertiesViaEditor.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // GAMEOBJECT SELECTION
                GameObject selectedObject = EditorGUILayout.ObjectField(
                    GUIContent.none,
                    (manualTracker.trackedPropertiesViaEditor[i].component)?.gameObject,
                    typeof(GameObject),
                    true
                ) as GameObject;

                // COMPONENT DROPDOWN
                if (selectedObject != null)
                {
                    // if the GameObject has changed, reset the component and property
                    if ((manualTracker.trackedPropertiesViaEditor[i].component)?.gameObject != selectedObject)
                    {
                        // pick transform component as default (might throw exceptions)
                        manualTracker.trackedPropertiesViaEditor[i].component =
                            selectedObject.GetComponent<Transform>();
                        manualTracker.trackedPropertiesViaEditor[i].propertyName = null;
                    }

                    // get all components on the GameObject
                    Component[] components = selectedObject.GetComponents<Component>();
                    List<string> componentNames = components.Select(comp => comp.GetType().Name).ToList();
                    int currentComponentIndex = components.ToList()
                        .IndexOf(manualTracker.trackedPropertiesViaEditor[i].component);
                    int newComponentIndex = EditorGUILayout.Popup(currentComponentIndex, componentNames.ToArray());

                    // update the selected component
                    if (newComponentIndex >= 0)
                    {
                        manualTracker.trackedPropertiesViaEditor[i].component = components[newComponentIndex];
                    }
                }

                // PROPERTY DROPDOWN
                if (manualTracker.trackedPropertiesViaEditor[i].component != null)
                {
                    // get list of properties
                    var properties = manualTracker.trackedPropertiesViaEditor[i].component.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Select(p => p.Name)
                        .ToList();

                    // get list of fields
                    var fields = manualTracker.trackedPropertiesViaEditor[i].component.GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .Select(f => f.Name)
                        .ToList();

                    // combine lists
                    var propertiesAndFields = properties.Concat(fields).ToList();

                    // find index of currently selected property
                    int currentIndex =
                        propertiesAndFields.IndexOf(manualTracker.trackedPropertiesViaEditor[i].propertyName);
                    if (currentIndex == -1) currentIndex = 0;

                    // select new property
                    int newIndex = EditorGUILayout.Popup(currentIndex, propertiesAndFields.ToArray());
                    manualTracker.trackedPropertiesViaEditor[i].propertyName = propertiesAndFields[newIndex];
                }

                // REMOVE BUTTON
                if (GUILayout.Button("Remove"))
                {
                    manualTracker.trackedPropertiesViaEditor.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Property"))
            {
                manualTracker.trackedPropertiesViaEditor.Add(new TrackedProperty());
            }

            // save changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manualTracker);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
