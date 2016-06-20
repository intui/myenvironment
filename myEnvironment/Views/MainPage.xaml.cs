using System;
using System.Linq;
using myEnvironment.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using static myEnvironment.Models.SensorModel;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;

namespace myEnvironment.Views
{
    public partial class MainPage : Page
    {
        List<Ambience> ambDataSet = new List<Ambience>();
        SensorContext db;
        int numBars;
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        protected long lastImport = 0;
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            if (localSettings.Values["lastImport"] == null)
            {
                localSettings.Values["lastImport"] = 0l;

            }
            else
            {
                lastImport = (long)localSettings.Values["lastImport"];
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadData();
            base.OnNavigatedTo(e);
        }
        public async void LoadData()
        {
            Busy.SetBusy(true, "loading data");

            await Task.Delay(400);
            db = new SensorContext();

            ambDataSet = db.AmbientDataSample.ToList(); //.Take(1000).ToList();
            numBars = ambDataSet.Count; // db.AmbientDataSample.Count();

            Busy.SetBusy(false);
            DataLoaded();
        }
        protected void DataLoaded()
        {
            int barSampleFactor = 3;


            barChartCanvas.Width = chartScoller.ActualWidth;
            barChartCanvas.Height = chartScoller.ActualHeight;
            barChartCanvas.Children.Clear();
            Random rand = new Random();
            Rectangle myRect;
            double barWidth = barChartCanvas.ActualWidth / numBars;
            numBars = (int) barChartCanvas.ActualWidth / barSampleFactor;
            barWidth = barSampleFactor;
            if (!db.AmbientDataSample.Any())
                return;
            DateTime fromTime = db.AmbientDataSample.Min(x => x.captureTime);
            DateTime ToTime = db.AmbientDataSample.Max(x => x.captureTime);
            TimeSpan timeSpan = ToTime - fromTime;
            decimal minTemp = db.AmbientDataSample.Where(x=>x.temperature > 0).Min(x => x.temperature); // > 0 temporary fix to eliminate error values - not valid in winter
            decimal maxTemp = db.AmbientDataSample.Max(x => x.temperature);
            decimal diffTemp = maxTemp - minTemp;
            decimal minHumi = db.AmbientDataSample.Min(x => x.humidity);
            decimal maxHumi = db.AmbientDataSample.Max(x => x.humidity);
            decimal diffHumi = maxHumi - minHumi;
            TimeSpan stepTime = new TimeSpan(((timeSpan).Ticks) / numBars);
            DateTime qTime = fromTime;
            Ambience queryAmbience;
            Ambience prevAmbience;

            prevAmbience = db.AmbientDataSample.OrderBy(x => x.captureTime).Where(x => x.captureTime > qTime).First();

            for (int i=1; i<numBars; i++)
            {
                qTime += stepTime;
                queryAmbience = db.AmbientDataSample.OrderBy(x => x.captureTime).Where(x => x.captureTime > qTime).First();
                myRect = new Rectangle();
                myRect.Width = ((queryAmbience.captureTime - prevAmbience.captureTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * barChartCanvas.ActualWidth; // + 0.2;

                if (queryAmbience?.temperature > 0)
                {
                    myRect.Height = ((double) (200 * ((queryAmbience.temperature - minTemp) / diffTemp)));
                }
                else
                    myRect.Height = 0;
                myRect.Fill = new SolidColorBrush(Colors.Purple);
                Canvas.SetLeft(myRect, ((prevAmbience.captureTime - fromTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * barChartCanvas.ActualWidth); // (barWidth) * i);
                Canvas.SetTop(myRect, barChartCanvas.ActualHeight - myRect.Height);
                barChartCanvas.Children.Add(myRect);
                
                myRect = new Rectangle();
                myRect.Width = ((queryAmbience.captureTime - prevAmbience.captureTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * barChartCanvas.ActualWidth; // + 0.2;
                if (queryAmbience?.humidity > 0)
                    myRect.Height = ((double)(200 * ((queryAmbience.humidity - minHumi) / diffHumi)));
                else
                    myRect.Height = 0;
                myRect.Fill = new SolidColorBrush(Colors.RoyalBlue);
                Canvas.SetLeft(myRect, ((prevAmbience.captureTime - fromTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * barChartCanvas.ActualWidth); // (barWidth) * i);
                Canvas.SetTop(myRect, barChartCanvas.ActualHeight / 2 - myRect.Height);
                //Canvas.SetZIndex(myRect, 1);
                //myRect.RenderTransform.
                barChartCanvas.Children.Add(myRect);

                prevAmbience = queryAmbience;
            }

            //Views.Busy.SetBusy(false);
        }
    }
}
