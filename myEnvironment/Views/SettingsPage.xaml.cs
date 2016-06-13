using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static myEnvironment.Models.SensorModel;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
//using Windows.Storage;

namespace myEnvironment.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
                MyPivot.SelectedIndex = index;
                using (var db = new SensorContext())
                {
                    Sensors.ItemsSource = db.Sensors.ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new SensorContext())
            {
                Sensor sensor;
                try
                {
                    sensor = new Sensor { SensorId = new System.Guid(NewSensor.Text), Title=NewSensor.Text, Location = "Here" };
                    db.Sensors.Add(sensor);
                    db.SaveChanges();
                }
                catch
                {
                    //sensor = new Sensor { SensorId = new System.Guid(), Location = "Here" };
                    NewSensor.Text = "not a valid Guid";
                }

                Sensors.ItemsSource = db.Sensors.ToList();
            }
        }
        private async void Get_Data(object sender, RoutedEventArgs e)
        {
            SensorContext db = new SensorContext();
            
            /**/
            try
            {
                string storageConnection = "DefaultEndpointsProtocol=https;AccountName=envirodata;AccountKey=Sqd5/SZpL2C3vip0JkeYt74s7xjSshV0v3kjlFSr36Ka21JJNhuER1JkteEfo2iGz2EK0EmcDnOvwPsojzcEMA==";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("default");
                BlobContinuationToken myToken = new BlobContinuationToken();
                //StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("UserDetails");
                //var results = container.ListBlobsSegmentedAsync(myToken);

                BlobContinuationToken continuationToken = null;
                List<IListBlobItem> results = new List<IListBlobItem>();
                do
                {
                    var response = await container.ListBlobsSegmentedAsync(continuationToken);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results);
                }
                while (continuationToken != null);

                NewSensor.Text = "sucessfully imported " + results.Count + " items.";
                int pastHours = Convert.ToInt16(Timespan.Text);
                TimeSpan pastSpan = new TimeSpan(0, pastHours, 0, 0);
                long fromTicks = DateTime.Now.Subtract(pastSpan).Ticks;
                
                Sensor sensor;
                string currentId = "";
                db.AmbientDataSample.RemoveRange(db.AmbientDataSample);
                db.SaveChanges();
                foreach (IListBlobItem item in results)
                {
                    if (Convert.ToInt64(item.Uri.ToString().Split('_')[1].Split('.')[0]) > fromTicks)
                    {
                        string blobText = await (item as CloudBlockBlob).DownloadTextAsync();
                        JArray jsonContent = JsonConvert.DeserializeObject(blobText) as JArray;
                        List<Ambience> ambBunch = new List<Ambience>();
                        foreach (JToken entry in jsonContent)
                        {
                            JObject myAmb = entry.ToObject<JObject>();
                            //myAmb.GetValue("ID");
                            currentId = myAmb.GetValue("ID").ToString();
                            NewSensor.Text = currentId;
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
                            //if (currentId.Equals("0"))
                              //  break;

                            //if (!db.AmbientDataSample.Where(x => x.ID == myAmb.GetValue("ID").ToObject<int>()).Any())
                            if(!currentId.Equals("0"))
                            {
                                Ambience myAmbience = new Ambience();
                                myAmbience.AmbienceId = (int)myAmb.GetValue("ID"); //.ToObject<int>();
                                myAmbience.captureTime = DateTime.Parse(myAmb.GetValue("captureTime").ToString());
                                myAmbience.temperature = myAmb.GetValue("temperature").ToObject<decimal>();
                                myAmbience.humidity = myAmb.GetValue("humidity").ToObject<decimal>();
                                myAmbience.Sensor = sensor;
                                //db.AmbientDataSample.Add(myAmbience);
                                ambBunch.Add(myAmbience);
                                //db.AmbientDataSample.AddRange()
                                //db.SaveChanges();
                            }
                        }
                        db.AmbientDataSample.AddRange(ambBunch);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                //db.SaveChanges();
                Debug.WriteLine(ex.Message);
            }
        /**/
        }
    }
}
