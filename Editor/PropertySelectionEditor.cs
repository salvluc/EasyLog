using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    public class PropertySelectionEditor
    {
        private bool _showTrackedProperties = true;
        public void Draw(Tracker tracker)
        {
            _showTrackedProperties = EditorGUILayout.Foldout(_showTrackedProperties, new GUIContent("Tracked Properties"), EditorStyles.foldoutHeader);

            if (_showTrackedProperties)
            {
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < tracker.trackedPropertiesViaEditor.Count; i++)
                {
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.BeginHorizontal();

                    // GAMEOBJECT SELECTION
                    GameObject selectedObject = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        (tracker.trackedPropertiesViaEditor[i].component)?.gameObject,
                        typeof(GameObject),
                        true
                    ) as GameObject;

                    // COMPONENT DROPDOWN
                    if (selectedObject != null)
                    {
                        // if the GameObject has changed, reset the component and property
                        if ((tracker.trackedPropertiesViaEditor[i].component)?.gameObject != selectedObject)
                        {
                            // pick transform component as default (might throw exceptions)
                            tracker.trackedPropertiesViaEditor[i].component = selectedObject.GetComponent<Transform>();
                            tracker.trackedPropertiesViaEditor[i].propertyName = null;
                        }

                        // get all components on the GameObject
                        Component[] components = selectedObject.GetComponents<Component>();
                        List<string> componentNames = components.Select(comp => comp.GetType().Name).ToList();
                        int currentComponentIndex = components.ToList().IndexOf(tracker.trackedPropertiesViaEditor[i].component);
                        int newComponentIndex = EditorGUILayout.Popup(currentComponentIndex, componentNames.ToArray());

                        // update the selected component
                        if (newComponentIndex >= 0)
                            tracker.trackedPropertiesViaEditor[i].component = components[newComponentIndex];
                    }

                    // PROPERTY DROPDOWN
                    if (tracker.trackedPropertiesViaEditor[i].component != null)
                    {
                        // get list of properties
                        var properties = tracker.trackedPropertiesViaEditor[i].component.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Select(p => p.Name)
                            .ToList();

                        // get list of fields
                        var fields = tracker.trackedPropertiesViaEditor[i].component.GetType()
                            .GetFields(BindingFlags.Instance | BindingFlags.Public)
                            .Select(f => f.Name)
                            .ToList();

                        // combine lists
                        var propertiesAndFields = properties.Concat(fields).ToList();

                        // find index of currently selected property
                        int currentIndex = propertiesAndFields.IndexOf(tracker.trackedPropertiesViaEditor[i].propertyName);
                        if (currentIndex == -1) currentIndex = 0;

                        // select new property
                        int newIndex = EditorGUILayout.Popup(currentIndex, propertiesAndFields.ToArray());
                        tracker.trackedPropertiesViaEditor[i].propertyName = propertiesAndFields[newIndex];
                    }

                    // REMOVE BUTTON
                    if (GUILayout.Button("Remove"))
                        tracker.trackedPropertiesViaEditor.RemoveAt(i);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Property"))
                    tracker.trackedPropertiesViaEditor.Add(new TrackedProperty());

                EditorGUI.indentLevel--;
            }
        }
    }
}
