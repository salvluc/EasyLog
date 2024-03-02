using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyLog
{
    [Serializable]
    public class InfluxUploader : OutputModule
    {
        public string url = "http://localhost:8086";
        public string org = "yourOrg";
        public string bucket = "bucket";
        public string apiToken = "yourAuthToken";

        public override string RequiredDataType { get; protected set; } = "INFLUX";
    
        private string InfluxDbUrl => $"{url}/api/v2/write?org={org}&bucket={bucket}&precision=s";

        public override void OnOutputRequested(string influxData)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(influxData);

            UnityWebRequest request = new UnityWebRequest(InfluxDbUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(byteData),
                downloadHandler = new DownloadHandlerBuffer()
            };
        
            request.SetRequestHeader("Authorization", "Token " + apiToken);
            request.SetRequestHeader("Content-Type", "text/plain");

            // Send the request and wait for the response
            request.SendWebRequest();

            while (!request.isDone)
            {
                // Optionally, add a timeout to prevent hanging in case of network issues
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("EasyLog: Influx upload failed: " + request.error);
            }
            else
            {
                Debug.Log("EasyLog: Influx upload successful: " + request.downloadHandler.text);
            }
        }
    }
}
