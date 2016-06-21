using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using static myEnvironment.Models.SensorModel;

namespace myEnvironment.Services.AzureServices.Blob
{
    class AzureBlobService
    {
        public static async Task<long> Get_Data(int pastHours, long lastUpdatedTicks = 0 )
        {
            return await Get_Data(pastHours, "default", lastUpdatedTicks);
        }
        public static async Task<long> Get_Data(int pastHours, string sensorId, long lastUpdatedTicks)
        { 
            SensorContext db = new SensorContext();
            long itemTicks = 0;
            try
            {
                var resources = ResourceLoader.GetForCurrentView("Resources");

                var blobStorageKey = resources.GetString("BlobStorageKey");

                string storageConnection = "DefaultEndpointsProtocol=https;AccountName=envirodata;AccountKey=" + blobStorageKey;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(sensorId);
                BlobContinuationToken myToken = new BlobContinuationToken();
                BlobContinuationToken continuationToken = null;
                List<IListBlobItem> results = new List<IListBlobItem>();
                do
                {
                    var response = await container.ListBlobsSegmentedAsync(continuationToken);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (continuationToken != null);
                TimeSpan pastSpan = new TimeSpan(0, pastHours, 0, 0);
                long fromTicks = DateTime.Now.Subtract(pastSpan).Ticks;
                
                Sensor sensor;
                string currentId = "";
                bool erasePreImport = false;
                if (erasePreImport)
                {
                    db.AmbientDataSample.RemoveRange(db.AmbientDataSample);
                    db.SaveChanges();
                }
                foreach (IListBlobItem item in results)
                {
                    itemTicks = Convert.ToInt64(item.Uri.ToString().Split('_')[1].Split('.')[0]);
                    if ( itemTicks > fromTicks && itemTicks > lastUpdatedTicks)
                    {
                        string blobText = await (item as CloudBlockBlob).DownloadTextAsync();
                        JArray jsonContent = JsonConvert.DeserializeObject(blobText) as JArray;
                        List<Ambience> ambBunch = new List<Ambience>();
                        foreach (JToken entry in jsonContent)
                        {
                            JObject myAmb = entry.ToObject<JObject>();
                            currentId = myAmb.GetValue("ID").ToString();
                            sensor = db.Sensors.Where(x => x.SensorId.ToString().Equals(myAmb.GetValue("sensorID").ToString())).FirstOrDefault();
                            if (sensor == null) // move out of foreach loop
                            {
                                try
                                {
                                    sensor = new Sensor { SensorId = new Guid(myAmb.GetValue("sensorID").ToString()), Title = "generic", Location = "Here" };
                                    db.Sensors.Add(sensor);
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }
                            if (!currentId.Equals("0"))
                            {
                                Ambience myAmbience = new Ambience();
                                myAmbience.AmbienceId = (int)myAmb.GetValue("ID"); //.ToObject<int>();
                                myAmbience.captureTime = DateTime.Parse(myAmb.GetValue("captureTime").ToString());
                                myAmbience.temperature = myAmb.GetValue("temperature").ToObject<decimal>();
                                myAmbience.humidity = myAmb.GetValue("humidity").ToObject<decimal>();
                                myAmbience.Sensor = sensor;
                                ambBunch.Add(myAmbience);
                            }
                        }
                        db.AmbientDataSample.AddRange(ambBunch);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return itemTicks;
        }
    }
}
