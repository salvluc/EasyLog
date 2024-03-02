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
            if (module is InfluxWriter influxWriter)
            {
                influxWriter.filePrefix = EditorGUILayout.TextField(
                    new GUIContent("File Prefix",
                        "The prefix of the saved file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
                    influxWriter.filePrefix);
                
                influxWriter.fileSuffix = EditorGUILayout.TextField(
                    new GUIContent("File Suffix",
                        "The suffix of the saved .csv file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
                    influxWriter.fileSuffix);

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
            
            if (module is CSVWriter csvWriter)
            {
                csvWriter.filePrefix = EditorGUILayout.TextField(
                    new GUIContent("File Prefix",
                        "The prefix of the saved file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
                    csvWriter.filePrefix);
                
                csvWriter.fileSuffix = EditorGUILayout.TextField(
                    new GUIContent("File Suffix",
                        "The suffix of the saved .csv file. The full file name will be:\nPrefix_dd-MM-yyyy_HH-mm_Suffix"),
                    csvWriter.fileSuffix);

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
            
            if (module is InfluxUploader influxUploader)
            {
                influxUploader.url = EditorGUILayout.TextField(new GUIContent("InfluxDB URL", "URL of the InfluxDB"), influxUploader.url);
                influxUploader.org = EditorGUILayout.TextField(new GUIContent("Organization", "Name of your InfluxDB organization"), influxUploader.org);
                influxUploader.bucket = EditorGUILayout.TextField(new GUIContent("Bucket", "Name of the target bucket"), influxUploader.bucket);
                influxUploader.apiToken = EditorGUILayout.TextField(new GUIContent("API Token", "Your InfluxDB API Token"), influxUploader.apiToken);
            }
        }
    }
}
