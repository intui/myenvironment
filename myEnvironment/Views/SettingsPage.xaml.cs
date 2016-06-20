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
using Windows.Storage;
//using Windows.Storage;

namespace myEnvironment.Views
{
    public sealed partial class SettingsPage : Page
    {
        Template10.Services.SerializationService.ISerializationService _SerializationService;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
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
        private async void Import_Data(object sender, RoutedEventArgs e)
        {
            Busy.SetBusy(true, "importing data");
            long lastUpdatedTicks = (long)localSettings.Values["lastImport"];
            localSettings.Values["lastImport"] = await Services.AzureServices.Blob.AzureBlobService.Get_Data(Convert.ToInt32(Timespan.Text), lastUpdatedTicks);
            Busy.SetBusy(false);
        }
    }
}
