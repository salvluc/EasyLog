using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class PropertySelectionEditor
    {
        private readonly Dictionary<Channel, bool> _trackedIntervalPropertiesFoldoutStates = new();
        
        public void Draw(Channel channel)
        {
            _trackedIntervalPropertiesFoldoutStates.TryAdd(channel, true); // Default state
            
            _trackedIntervalPropertiesFoldoutStates[channel] = EditorGUILayout.Foldout(
                _trackedIntervalPropertiesFoldoutStates[channel], "Tracked Properties", EditorStyles.foldoutHeader);

            if (!_trackedIntervalPropertiesFoldoutStates[channel])
                return;
            
            Color cachedColor = GUI.backgroundColor;
            
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
                
                GUI.backgroundColor = StyleKit.RemoveColor;
                
                // REMOVE BUTTON
                if (GUILayout.Button("x"))
                    channel.trackedPropertiesViaEditor.RemoveAt(i);
                
                GUI.backgroundColor = cachedColor;
                    
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel = cachedIndent;
            }
            
            GUI.backgroundColor = StyleKit.CsvColor;
                
            if (GUILayout.Button("Add Property"))
                channel.trackedPropertiesViaEditor.Add(new TrackedEditorProperty());

            GUI.backgroundColor = cachedColor;
        }
    }
}
