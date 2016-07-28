using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.Dht;
using Sensors.OneWire.Common;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Data.Json;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using System.Diagnostics;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace Sensors.OneWire
{
	public sealed partial class MainPage : BindablePage
    {
        private DispatcherTimer _timer = new DispatcherTimer();
        private DispatcherTimer storageTimer = new DispatcherTimer();

        GpioPin _pin4 = null;
        GpioPin _pin17 = null;
        private IDht _dht1 = null;
        private IDht _dht2 = null;
        private List<int> _retryCount = new List<int>();
        private DateTimeOffset _startedAt = DateTimeOffset.MinValue;
        private List<DataItem> itemList = new List<DataItem>();
        int lastID;
        Guid sensorID = Guid.NewGuid();
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder; //.RoamingFolder;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        CloudBlobContainer container;
        bool readSensor2 = false;
        bool GpioControllerPresent;
        string DeviceConnectionString;
        DeviceClient deviceClient;

        public MainPage()
        {
            this.InitializeComponent();
            storageTimer.Interval = TimeSpan.FromMinutes(60);
            storageTimer.Tick += StorageTimer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(60);
            _timer.Tick += _timer_Tick;
            //???
            GpioControllerPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController");
            if (localSettings.Values["sensorId"] == null)
            {
                sensorID = Guid.NewGuid();
                localSettings.Values["sensorId"] = sensorID;
                lastID = 0;
                localSettings.Values["lastId"] = 0;
            }
            else
            {
                sensorID = (Guid) localSettings.Values["sensorId"];
                lastID = (int)localSettings.Values["lastId"];
            }


        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (GpioControllerPresent && GpioController.GetDefault()!= null)
            {
                _pin4 = GpioController.GetDefault().OpenPin(4, GpioSharingMode.Exclusive);
                
                _dht1 = new Dht22(_pin4, GpioPinDriveMode.Input);
                if (readSensor2)
                {
                    _dht2 = new Dht11(_pin17, GpioPinDriveMode.Input);
                    _pin17 = GpioController.GetDefault().OpenPin(17, GpioSharingMode.Exclusive);
                }
                _timer.Start();
                _startedAt = DateTimeOffset.Now;
                Task.Delay(1000);
                storageTimer.Start();
            }
            else
            {
                Humi2.Text = "no sensor found.";
            }
            //if(!deviceClient...) say: regiser your device.

            var resources = ResourceLoader.GetForCurrentView("Resources");
            var blobStorageKey = resources.GetString("BlobStorageKey");
            string storageConnection = "DefaultEndpointsProtocol=https;AccountName=envirodata;AccountKey=" + blobStorageKey;
            //string storageConnection = "***replace with your azure connection string***";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnection);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("default"); // container name = sensorId - from Alljoyn (create container in Azure if not exists)

            DeviceConnectionString = "DeviceId=" + sensorID + ";" + resources.GetString("IotHubCS");
            deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();

            _pin4.Dispose();
            _pin4 = null;
            _pin17.Dispose();
            _pin17 = null;

            _dht1 = null;
            _dht2 = null;

            base.OnNavigatedFrom(e);
        }

        private async void _timer_Tick(object sender, object e)
        {
            DhtReading reading = new DhtReading();
            int val = this.TotalAttempts;
            this.TotalAttempts++;

            reading = await _dht1.GetReadingAsync(30).AsTask();

            _retryCount.Add(reading.RetryCount);
            this.OnPropertyChanged(nameof(AverageRetriesDisplay));
            this.OnPropertyChanged(nameof(TotalAttempts));
            this.OnPropertyChanged(nameof(PercentSuccess));

			if (reading.IsValid)
			{
				this.TotalSuccess++;
				this.Temperature = Convert.ToSingle(reading.Temperature);
				this.Humidity = Convert.ToSingle(reading.Humidity);
				this.LastUpdated = DateTimeOffset.Now;
				this.OnPropertyChanged(nameof(SuccessRate));
                DataItem myItem = new DataItem(lastID++, sensorID, DateTime.Now, (float)reading.Temperature, (float)reading.Humidity);
                itemList.Add(myItem);
                try
                {
                    sendToIotHub(myItem);
                }
                catch(Exception ex) //DeviceNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            this.OnPropertyChanged(nameof(LastUpdatedDisplay));

            if (readSensor2)
            {
                reading = new DhtReading();
                reading = await _dht2.GetReadingAsync(30).AsTask();
                if (reading.IsValid)
                {
                    Humi2.Text = reading.Humidity.ToString("F2");
                    Temp2.Text = reading.Temperature.ToString("F2");
                }
            }
        }

        private async void sendToIotHub(DataItem myItem)
        {
            var jsonString = JsonConvert.SerializeObject(myItem);
            jsonString = myItem.Stringify(); //??
            var jsonStringInBytes = new Message(Encoding.ASCII.GetBytes(jsonString));
            await deviceClient.SendEventAsync(jsonStringInBytes);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.UtcNow, jsonString);
        }

        private async void StorageTimer_Tick(object sender, object e)
        {
            if (itemList.Count == 0)
                return;
            string filename = "envData_" + itemList[0].captureTime.Ticks.ToString() + ".json";
            StorageFile file = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            var localfiles = await storageFolder.GetFilesAsync();
            //Temp2.Text += "\nFilecount: " + localfiles.Count;
            //string jsonContent =  Newtonsoft.Json.JsonConvert.SerializeObject(itemList[0]);
            JsonArray itemsJson = new JsonArray();
            foreach (DataItem item in itemList )
            {
                itemsJson.Add(item.ToJsonObject());
            }
            
            string jsonContent = itemsJson.Stringify();

            // upload to Azure Blob
            //using (var fileStream = System.IO.File.OpenRead)
            try {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
                await blockBlob.UploadTextAsync(jsonContent);
                await FileIO.AppendTextAsync(file, jsonContent);
                localSettings.Values["lastId"] = lastID;
            }
            catch (Exception ex)
            {
                // log exception
                // move filename to upload stack
            }
            itemList.Clear();
            //_dht1 = new Dht22(_pin4, GpioPinDriveMode.Input);
            //_dht2 = new Dht11(_pin17, GpioPinDriveMode.Input);

        }

        public string PercentSuccess
        {
            get
            {
                string returnValue = string.Empty;

                int attempts = this.TotalAttempts;

                if (attempts > 0)
                {
                    returnValue = string.Format("{0:0.0}%", 100f * (float)this.TotalSuccess / (float)attempts);
                }
                else
                {
                    returnValue = "0.0%";
                }

                return returnValue;
            }
        }

        private int _totalAttempts = 0;
        public int TotalAttempts
        {
            get
            {
                return _totalAttempts;
            }
            set
            {
                this.SetProperty(ref _totalAttempts, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private int _totalSuccess = 0;
        public int TotalSuccess
        {
            get
            {
                return _totalSuccess;
            }
            set
            {
                this.SetProperty(ref _totalSuccess, value);
                this.OnPropertyChanged(nameof(PercentSuccess));
            }
        }

        private float _humidity = 0f;
        public float Humidity
        {
            get
            {
                return _humidity;
            }

            set
            {
                this.SetProperty(ref _humidity, value);
                this.OnPropertyChanged(nameof(HumidityDisplay));
            }
        }

        public string HumidityDisplay
        {
            get
            {
                return string.Format("{0:0.0}% RH", this.Humidity);
            }
        }

        private float _temperature = 0f;
        public float Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                this.SetProperty(ref _temperature, value);
                this.OnPropertyChanged(nameof(TemperatureDisplay));
            }
        }

        public string TemperatureDisplay
        {
            get
            {
                return string.Format("{0:0.0} °C", this.Temperature);
            }
        }

        private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
        public DateTimeOffset LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                this.SetProperty(ref _lastUpdated, value);
                this.OnPropertyChanged(nameof(LastUpdatedDisplay));
            }
        }

        public string LastUpdatedDisplay
        {
            get
            {
                string returnValue = string.Empty;

                TimeSpan elapsed = DateTimeOffset.Now.Subtract(this.LastUpdated);

                if (this.LastUpdated == DateTimeOffset.MinValue)
                {
                    returnValue = "never";
                }
                else if (elapsed.TotalSeconds < 60d)
                {
                    int seconds = (int)elapsed.TotalSeconds;

                    if (seconds < 2)
                    {
                        returnValue = "just now";
                    }
                    else
                    {
                        returnValue = string.Format("{0:0} {1} ago", seconds, seconds == 1 ? "second" : "seconds");
                    }
                }
                else if (elapsed.TotalMinutes < 60d)
                {
                    int minutes = (int)elapsed.TotalMinutes == 0 ? 1 : (int)elapsed.TotalMinutes;
                    returnValue = string.Format("{0:0} {1} ago", minutes, minutes == 1 ? "minute" : "minutes");
                }
                else if (elapsed.TotalHours < 24d)
                {
                    int hours = (int)elapsed.TotalHours == 0 ? 1 : (int)elapsed.TotalHours;
                    returnValue = string.Format("{0:0} {1} ago", hours, hours == 1 ? "hour" : "hours");
                }
                else
                {
                    returnValue = "a long time ago";
                }

                return returnValue;
            }
        }

        public int AverageRetries
        {
            get
            {
                int returnValue = 0;

                if (_retryCount.Count() > 0)
                {
                    returnValue = (int)_retryCount.Average();
                }

                return returnValue;
            }
        }

        public string AverageRetriesDisplay
        {
            get
            {
                return string.Format("{0:0}", this.AverageRetries);
            }
        }

        public string SuccessRate
        {
            get
            {
                string returnValue = string.Empty;

                double totalSeconds = DateTimeOffset.Now.Subtract(_startedAt).TotalSeconds;
                double rate = this.TotalSuccess / totalSeconds;

                if (rate < 1)
                {
                    returnValue = string.Format("{0:0.00} seconds/reading", 1d / rate);
                }
                else
                {
                    returnValue = string.Format("{0:0.00} readings/sec", rate);
                }

                return returnValue;
            }
        }
    }
}
