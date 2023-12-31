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
        private readonly Dictionary<ManualChannel, bool> _trackedManualPropertiesFoldoutStates = new();
        
        public void DrawInterval(IntervalChannel channel)
        {
            _trackedIntervalPropertiesFoldoutStates.TryAdd(channel, true); // Default state
            
            _trackedIntervalPropertiesFoldoutStates[channel] = EditorGUILayout.Foldout(
                _trackedIntervalPropertiesFoldoutStates[channel], "Tracked Properties", EditorStyles.foldoutHeader);

            if (!_trackedIntervalPropertiesFoldoutStates[channel])
                return;
            
            for (int i = 0; i < channel.trackedPropertiesViaEditor.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // GAMEOBJECT SELECTION
                GameObject selectedObject = EditorGUILayout.ObjectField(
                    GUIContent.none,
                    (channel.trackedPropertiesViaEditor[i].component)?.gameObject,
                    typeof(GameObject),
                    true
                ) as GameObject;

                var cachedIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

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
                    
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel = cachedIndent;
            }
                
            if (GUILayout.Button("Add Property"))
                channel.trackedPropertiesViaEditor.Add(new TrackedProperty());
        }

        public void DrawManual(ManualChannel channel)
        {
            _trackedManualPropertiesFoldoutStates.TryAdd(channel, true); // Default state
            
            _trackedManualPropertiesFoldoutStates[channel] = EditorGUILayout.Foldout(
                _trackedManualPropertiesFoldoutStates[channel], "Tracked Properties", EditorStyles.foldoutHeader);

            if (!_trackedManualPropertiesFoldoutStates[channel])
                return;
            
            for (int i = 0; i < channel.trackedPropertiesViaEditor.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // GAMEOBJECT SELECTION
                GameObject selectedObject = EditorGUILayout.ObjectField(
                    GUIContent.none,
                    (channel.trackedPropertiesViaEditor[i].component)?.gameObject,
                    typeof(GameObject),
                    true
                ) as GameObject;

                var cachedIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

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
                    
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel = cachedIndent;
            }
                
            if (GUILayout.Button("Add Property"))
                channel.trackedPropertiesViaEditor.Add(new TrackedProperty());
        }
    }
}
