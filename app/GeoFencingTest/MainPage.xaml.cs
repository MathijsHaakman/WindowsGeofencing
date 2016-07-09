using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Storage;

using Windows.UI.Core;
using Windows.Globalization.DateTimeFormatting;
using Windows.Globalization;
using Windows.Globalization.NumberFormatting;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeoFencingTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IList<Geofence> geofences = new List<Geofence>();

        public MainPage()
        {
            this.InitializeComponent();
            InitializeGeolocation();
            try
            {
                String fenceId = "geofence21";
                BasicGeoposition position;
                position.Latitude = 52.690529;
                position.Longitude = 5.183422;
                position.Altitude = 0.0;
                double radius = 200;
                Geocircle geocircle = new Geocircle(position, radius);
                bool singleUse = false;
                MonitoredGeofenceStates mask = MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Exited | MonitoredGeofenceStates.Removed;
                TimeSpan dwellTime = TimeSpan.FromSeconds(5);
                TimeSpan duration = TimeSpan.FromHours(1);
                DateTimeOffset startTime = DateTime.Now;
                Geofence geofence = new Geofence(fenceId, geocircle, mask, singleUse, dwellTime, startTime, duration);
                geofences.Add(geofence);
                RegisterBackgroundTask();
            } catch(Exception e)
            {
                Debug.WriteLine(e, "error");
            }
        }

        private async void InitializeGeolocation()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if(accessStatus == GeolocationAccessStatus.Allowed) { 
                geofences = GeofenceMonitor.Current.Geofences;
                Debug.WriteLine(GeofenceMonitor.Current, "geofences");
                GeofenceMonitor.Current.GeofenceStateChanged += OnGeofenceStateChanged;
                GeofenceMonitor.Current.StatusChanged += OnGeofenceStatusChanged;
            }
        }

        private void OnGeofenceStateChanged(GeofenceMonitor sender, object args)
        {
            Debug.WriteLine(sender, "state changed");
        }

        private void OnGeofenceStatusChanged(GeofenceMonitor sender, object args)
        {
            Debug.WriteLine(sender, "status changed");
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            GeofenceMonitor.Current.GeofenceStateChanged -= OnGeofenceStateChanged;
            GeofenceMonitor.Current.StatusChanged -= OnGeofenceStatusChanged;

            base.OnNavigatingFrom(e);
        }

        async private void RegisterBackgroundTask()
        {
            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();

            geofenceTaskBuilder.Name = "geofenceTaskBuilderName";
            geofenceTaskBuilder.TaskEntryPoint = "BackgroundTask.GeofenceBackgroundTask";

            var trigger = new LocationTrigger(LocationTriggerType.Geofence);
            geofenceTaskBuilder.SetTrigger(trigger);

            IBackgroundTaskRegistration geofenceTask = geofenceTaskBuilder.Register();
            geofenceTask.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

            
        }

        async private void OnCompleted(IBackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs e)
        {
            if(sender != null)
            {
                DoToast("Completed");
            }
        }

        private void DoToast(string eventName)
        {
            // pop a toast for each geofence event
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();

            // Create a two line toast and add audio reminder

            // Here the xml that will be passed to the 
            // ToastNotification for the toast is retrieved
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            // Set both lines of text
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode("Geolocation Sample"));

            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(eventName));
            string secondLine = "There are new geofence events";
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(secondLine));

            // now create a xml node for the audio source
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotifier.Show(toast);
        }

    }
}
