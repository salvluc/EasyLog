using System;
using System.IO;
using UnityEngine;

namespace EasyLog
{
    public static class FileUtility
    {
        public static void SaveFile(string filePath, string data)
        {
            try
            {
                if (!IsValidPath(filePath))
                {
                    Debug.LogError("EasyLog: Invalid file path.");
                    return;
                }

                WriteDataToFile(filePath, data);
            }
            catch (Exception ex)
            {
                Debug.LogError("EasyLog: Error saving file: " + ex.Message);
                TryWriteToDefaultDirectory(filePath, data);
            }
        }

        private static void WriteDataToFile(string filePath, string data)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                if (directory != null)
                    Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, data);
        }

        private static void TryWriteToDefaultDirectory(string filePath, string data)
        {
            try
            {
                string defaultDirectory = Application.dataPath + "/EasyLog";
                string defaultFilePath = Path.Combine(defaultDirectory, Path.GetFileName(filePath));

                WriteDataToFile(defaultFilePath, data);

                Debug.Log("EasyLog: Data was saved in ./EasyLog");
            }
            catch (Exception ex)
            {
                Debug.LogError("EasyLog: Fallback to default directory unsuccessful: " + ex.Message);
            }
        }

        private static bool IsValidPath(string filePath)
        {
            try
            {
                string test = Path.GetFullPath(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
