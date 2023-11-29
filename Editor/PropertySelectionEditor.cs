using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyLog.Core;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class PropertySelectionEditor
    {
        private readonly Dictionary<IntervalChannel, bool> _trackedIntervalPropertiesFoldoutStates = new();
        private readonly Dictionary<IntervalChannel, bool> _trackedManualPropertiesFoldoutStates = new();
        
        public void DrawInterval(IntervalChannel channel)
        {
            _trackedIntervalPropertiesFoldoutStates.TryAdd(channel, true); // Default state
            
            _trackedIntervalPropertiesFoldoutStates[channel] = EditorGUILayout.Foldout(
                _trackedIntervalPropertiesFoldoutStates[channel], "Tracked Properties", EditorStyles.foldoutHeader);

            if (_trackedIntervalPropertiesFoldoutStates[channel])
            {
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < channel.trackedPropertiesViaEditor.Count; i++)
                {
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.BeginHorizontal();

                    // GAMEOBJECT SELECTION
                    GameObject selectedObject = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        (channel.trackedPropertiesViaEditor[i].component)?.gameObject,
                        typeof(GameObject),
                        true
                    ) as GameObject;

                    // COMPONENT DROPDOWN
                    if (selectedObject != null)
                    {
                        // if the GameObject has changed, reset the component and property
                        if ((channel.trackedPropertiesViaEditor[i].component)?.gameObject != selectedObject)
                        {
                            // pick transform component as default (might throw exceptions)
                            channel.trackedPropertiesViaEditor[i].component = selectedObject.GetComponent<Transform>();
                            channel.trackedPropertiesViaEditor[i].propertyName = null;
                        }

                        // get all components on the GameObject
                        Component[] components = selectedObject.GetComponents<Component>();
                        List<string> componentNames = components.Select(comp => comp.GetType().Name).ToList();
                        int currentComponentIndex = components.ToList().IndexOf(channel.trackedPropertiesViaEditor[i].component);
                        int newComponentIndex = EditorGUILayout.Popup(currentComponentIndex, componentNames.ToArray());

                        // update the selected component
                        if (newComponentIndex >= 0)
                            channel.trackedPropertiesViaEditor[i].component = components[newComponentIndex];
                    }

                    // PROPERTY DROPDOWN
                    if (channel.trackedPropertiesViaEditor[i].component != null)
                    {
                        // get list of properties
                        var properties = channel.trackedPropertiesViaEditor[i].component.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Select(p => p.Name)
                            .ToList();

                        // get list of fields
                        var fields = channel.trackedPropertiesViaEditor[i].component.GetType()
                            .GetFields(BindingFlags.Instance | BindingFlags.Public)
                            .Select(f => f.Name)
                            .ToList();

                        // combine lists
                        var propertiesAndFields = properties.Concat(fields).ToList();

                        // find index of currently selected property
                        int currentIndex = propertiesAndFields.IndexOf(channel.trackedPropertiesViaEditor[i].propertyName);
                        if (currentIndex == -1) currentIndex = 0;

                        // select new property
                        int newIndex = EditorGUILayout.Popup(currentIndex, propertiesAndFields.ToArray());
                        channel.trackedPropertiesViaEditor[i].propertyName = propertiesAndFields[newIndex];
                    }

                    // REMOVE BUTTON
                    if (GUILayout.Button("Remove"))
                        channel.trackedPropertiesViaEditor.RemoveAt(i);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Property"))
                    channel.trackedPropertiesViaEditor.Add(new TrackedProperty());

                EditorGUI.indentLevel--;
            }
        }
        /*
        public void DrawManual(ManualTracker tracker)
        {
            _trackedIntervalPropertiesFoldoutStates[channel] = EditorGUILayout.Foldout(
                _trackedIntervalPropertiesFoldoutStates[channel], "Tracked Properties", EditorStyles.foldoutHeader);

            if (_trackedIntervalPropertiesFoldoutStates[channel])
            {
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < tracker.GetChannel().trackedPropertiesViaEditor.Count; i++)
                {
                    EditorGUI.indentLevel--;
                    
                    EditorGUILayout.BeginHorizontal();

                    // GAMEOBJECT SELECTION
                    GameObject selectedObject = EditorGUILayout.ObjectField(
                        GUIContent.none,
                        (tracker.GetChannel().trackedPropertiesViaEditor[i].component)?.gameObject,
                        typeof(GameObject),
                        true
                    ) as GameObject;

                    // COMPONENT DROPDOWN
                    if (selectedObject != null)
                    {
                        // if the GameObject has changed, reset the component and property
                        if ((tracker.GetChannel().trackedPropertiesViaEditor[i].component)?.gameObject != selectedObject)
                        {
                            // pick transform component as default (might throw exceptions)
                            tracker.GetChannel().trackedPropertiesViaEditor[i].component = selectedObject.GetComponent<Transform>();
                            tracker.GetChannel().trackedPropertiesViaEditor[i].propertyName = null;
                        }

                        // get all components on the GameObject
                        Component[] components = selectedObject.GetComponents<Component>();
                        List<string> componentNames = components.Select(comp => comp.GetType().Name).ToList();
                        int currentComponentIndex = components.ToList().IndexOf(tracker.GetChannel().trackedPropertiesViaEditor[i].component);
                        int newComponentIndex = EditorGUILayout.Popup(currentComponentIndex, componentNames.ToArray());

                        // update the selected component
                        if (newComponentIndex >= 0)
                            tracker.GetChannel().trackedPropertiesViaEditor[i].component = components[newComponentIndex];
                    }

                    // PROPERTY DROPDOWN
                    if (tracker.GetChannel().trackedPropertiesViaEditor[i].component != null)
                    {
                        // get list of properties
                        var properties = tracker.GetChannel().trackedPropertiesViaEditor[i].component.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Select(p => p.Name)
                            .ToList();

                        // get list of fields
                        var fields = tracker.GetChannel().trackedPropertiesViaEditor[i].component.GetType()
                            .GetFields(BindingFlags.Instance | BindingFlags.Public)
                            .Select(f => f.Name)
                            .ToList();

                        // combine lists
                        var propertiesAndFields = properties.Concat(fields).ToList();

                        // find index of currently selected property
                        int currentIndex = propertiesAndFields.IndexOf(tracker.GetChannel().trackedPropertiesViaEditor[i].propertyName);
                        if (currentIndex == -1) currentIndex = 0;

                        // select new property
                        int newIndex = EditorGUILayout.Popup(currentIndex, propertiesAndFields.ToArray());
                        tracker.GetChannel().trackedPropertiesViaEditor[i].propertyName = propertiesAndFields[newIndex];
                    }

                    // REMOVE BUTTON
                    if (GUILayout.Button("Remove"))
                        tracker.GetChannel().trackedPropertiesViaEditor.RemoveAt(i);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Property"))
                    tracker.GetChannel().trackedPropertiesViaEditor.Add(new TrackedProperty());

                EditorGUI.indentLevel--;
            }
        }*/
    }
}
