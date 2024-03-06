using System;
using UnityEditor;
using UnityEngine;

namespace EasyLog.Editor
{
    [Serializable]
    public class OutputModuleEditor
    {
        public void Draw(OutputModule module)
        {
            switch (module)
            {
                case InfluxWriter influxWriter:
                {
                    influxWriter.filePrefix = EditorGUILayout.TextField(
                        new GUIContent("File Prefix",
                            "The prefix of the saved file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        influxWriter.filePrefix);
                
                    influxWriter.fileSuffix = EditorGUILayout.TextField(
                        new GUIContent("File Suffix",
                            "The suffix of the saved .csv file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        influxWriter.fileSuffix);
                    
                    influxWriter.useStandardSaveLocation = EditorGUILayout.Toggle(new GUIContent("Use Relative Location",
                            "File will be saved in: " + "\"" + ".../EasyLog" + "\"" + ". Useful for builds."),
                        influxWriter.useStandardSaveLocation);

                    if (!influxWriter.useStandardSaveLocation)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("Save Location", influxWriter.saveLocation);

                        if (GUILayout.Button("Open",GUILayout.Width(45)))
                        {
                            Application.OpenURL(influxWriter.saveLocation);
                        }
                
                        if (GUILayout.Button("Change", GUILayout.Width(55)))
                        {
                            var newSaveLocation = EditorUtility.OpenFolderPanel("Select Save Location", influxWriter.saveLocation, "");
                            influxWriter.saveLocation = newSaveLocation == "" ? influxWriter.saveLocation : newSaveLocation;
                        }
                
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    break;
                }
                case CSVWriter csvWriter:
                {
                    csvWriter.filePrefix = EditorGUILayout.TextField(
                        new GUIContent("File Prefix",
                            "The prefix of the saved file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        csvWriter.filePrefix);
                
                    csvWriter.fileSuffix = EditorGUILayout.TextField(
                        new GUIContent("File Suffix",
                            "The suffix of the saved .csv file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        csvWriter.fileSuffix);
                
                    string delimiter = EditorGUILayout.TextField(
                        new GUIContent("Delimiter",
                            "The delimiter used to separate the values"),
                        csvWriter.delimiter.ToString());
                
                    if (!string.IsNullOrEmpty(delimiter))
                        csvWriter.delimiter = StyleKit.StringToChar(delimiter);
                
                    string delimiterReplacement = EditorGUILayout.TextField(
                        new GUIContent("Delimiter Replacement",
                            "If the delimiter is part of a tracked value, it will be replaced with this to keep columns separated"),
                        csvWriter.delimiterReplacement.ToString());
                
                    if (!string.IsNullOrEmpty(delimiterReplacement))
                        csvWriter.delimiterReplacement = StyleKit.StringToChar(delimiterReplacement);
                    
                    csvWriter.useStandardSaveLocation = EditorGUILayout.Toggle(new GUIContent("Use Relative Location",
                            "File will be saved in: " + "\"" + ".../EasyLog" + "\"" + ". Useful for builds."),
                        csvWriter.useStandardSaveLocation);

                    if (!csvWriter.useStandardSaveLocation)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("Save Location", csvWriter.saveLocation);

                        if (GUILayout.Button("Open",GUILayout.Width(45)))
                        {
                            Application.OpenURL(csvWriter.saveLocation);
                        }
                
                        if (GUILayout.Button("Change", GUILayout.Width(55)))
                        {
                            var newSaveLocation = EditorUtility.OpenFolderPanel("Select Save Location", csvWriter.saveLocation, "");
                            csvWriter.saveLocation = newSaveLocation == "" ? csvWriter.saveLocation : newSaveLocation;
                        }
                
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    break;
                }
                
                case SystemInfoWriter systemInfoWriter:
                {
                    systemInfoWriter.filePrefix = EditorGUILayout.TextField(
                        new GUIContent("File Prefix",
                            "The prefix of the saved file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        systemInfoWriter.filePrefix);
                
                    systemInfoWriter.fileSuffix = EditorGUILayout.TextField(
                        new GUIContent("File Suffix",
                            "The suffix of the saved file. The full file name will be:\n<Prefix>yyyy-MM-dd_HH-mm<Suffix>"),
                        systemInfoWriter.fileSuffix);
                    
                    systemInfoWriter.useStandardSaveLocation = EditorGUILayout.Toggle(new GUIContent("Use Relative Location",
                            "File will be saved in: " + "\"" + ".../EasyLog" + "\"" + ". Useful for builds."),
                        systemInfoWriter.useStandardSaveLocation);

                    if (!systemInfoWriter.useStandardSaveLocation)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("Save Location", systemInfoWriter.saveLocation);

                        if (GUILayout.Button("Open",GUILayout.Width(45)))
                        {
                            Application.OpenURL(systemInfoWriter.saveLocation);
                        }
                
                        if (GUILayout.Button("Change", GUILayout.Width(55)))
                        {
                            var newSaveLocation = EditorUtility.OpenFolderPanel("Select Save Location", systemInfoWriter.saveLocation, "");
                            systemInfoWriter.saveLocation = newSaveLocation == "" ? systemInfoWriter.saveLocation : newSaveLocation;
                        }
                
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    break;
                }
            }

            if (module is not InfluxUploader influxUploader) return;
            
            influxUploader.url = EditorGUILayout.TextField(new GUIContent("InfluxDB URL", "URL of the InfluxDB"), influxUploader.url);
            influxUploader.apiToken = EditorGUILayout.TextField(new GUIContent("API Token", "Your InfluxDB API Token"), influxUploader.apiToken);
            influxUploader.org = EditorGUILayout.TextField(new GUIContent("Organization", "Name of your InfluxDB organization"), influxUploader.org);
            influxUploader.bucket = EditorGUILayout.TextField(new GUIContent("Bucket", "Name of the target bucket"), influxUploader.bucket);

            EditorGUILayout.BeginHorizontal();
                
            if (GUILayout.Button("Test Connection"))
                influxUploader.TestConnection();
                
            if (GUILayout.Button("Test Bucket"))
                influxUploader.TestBucket();
                
            EditorGUILayout.EndHorizontal();
        }
    }
}
