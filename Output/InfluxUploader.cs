using System;
using System.Diagnostics;
using System.Text;
using UnityEngine.Networking;
using SimpleJSON;
using Debug = UnityEngine.Debug;

namespace EasyLog
{
    [Serializable]
    public class InfluxUploader : OutputModule
    {
        public string url = "http://localhost:8086";
        public string org = "yourOrg";
        public string bucket = "yourBucket";
        public string apiToken = "yourAuthToken";
    
        private string InfluxDbUrl => $"{url}/api/v2/write?org={org}&bucket={bucket}&precision=ms";

        public override void OnOutputRequested(string influxData, string channelName)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(influxData);

            UnityWebRequest request = new UnityWebRequest(InfluxDbUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(byteData),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Authorization", "Token " + apiToken);
            request.SetRequestHeader("Content-Type", "text/plain");
            request.SendWebRequest();
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            const float timeout = 10f;

            while (!request.isDone)
            {
                if (!(stopwatch.ElapsedMilliseconds > timeout * 1000)) continue;
                
                Debug.LogError("EasyLog: InfluxDB connection timeout.");
                break;
            }
            
            stopwatch.Stop();
            
            if (!request.isDone) return;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("EasyLog: InfluxDB upload failed: " + request.error);
            }
            else
            {
                Debug.Log("EasyLog: InfluxDB upload successful: " + request.downloadHandler.text);
            }
        }
        
        public void TestConnection()
        {
            string bucketsEndpoint = url + "/api/v2/buckets?org=" + org;

            UnityWebRequest request = UnityWebRequest.Get(bucketsEndpoint);
            request.SetRequestHeader("Authorization", "Token " + apiToken);
            request.SendWebRequest();
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            const float timeout = 10f;

            while (!request.isDone)
            {
                if (!(stopwatch.ElapsedMilliseconds > timeout * 1000)) continue;
                
                Debug.LogError("EasyLog: InfluxDB connection timeout.");
                break;
            }
            
            stopwatch.Stop();

            if (!request.isDone) return;
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("EasyLog: InfluxDB connection failed: " + request.error);
            }
            else
            {
                Debug.Log("EasyLog: InfluxDB connection successful!");
            }
        }
        
        public void TestBucket()
        {
            string bucketsEndpoint = $"{url}/api/v2/buckets?org={org}";

            UnityWebRequest request = UnityWebRequest.Get(bucketsEndpoint);
            request.SetRequestHeader("Authorization", "Token " + apiToken);
            request.SendWebRequest();
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            const float timeout = 10f; // Timeout in seconds

            while (!request.isDone)
            {
                if (!(stopwatch.ElapsedMilliseconds > timeout * 1000)) continue;
                
                Debug.LogError("EasyLog: InfluxDB connection timeout.");
                break;
            }
            
            stopwatch.Stop();

            if (!request.isDone) return;
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("EasyLog: Error fetching buckets: " + request.error);
            }
            else
            {
                bool bucketExists = CheckIfBucketExists(request.downloadHandler.text);
                if (bucketExists)
                {
                    Debug.Log("EasyLog: Bucket found!");
                }
                else
                {
                    Debug.LogError($"EasyLog: Bucket \"{bucket}\" not found in org \"{org}\".");
                }
            }
        }
        
        private bool CheckIfBucketExists(string jsonResponse)
        {
            var json = JSON.Parse(jsonResponse);
            var bucketsArray = json["buckets"].AsArray;

            foreach (JSONNode bucketNode in bucketsArray)
            {
                if (bucketNode["name"].Value == bucket)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
