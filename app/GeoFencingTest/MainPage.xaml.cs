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
        IBackgroundTaskRegistration geofenceTask;
        private IList<Geofence> geofences = new List<Geofence>();

        public MainPage()
        {
            this.InitializeComponent();
            InitializeGeolocation();
            
        }

        private async void InitializeGeolocation()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if(accessStatus == GeolocationAccessStatus.Allowed) { 
                geofences = GeofenceMonitor.Current.Geofences;
                Debug.WriteLine(GeofenceMonitor.Current, "geofences");
                try
                {
                    String fenceId = "Appedemic";
                    BasicGeoposition position;
                    position.Latitude = 52.492187;
                    position.Longitude = 4.797308;
                    position.Altitude = 0.0;
                    Double radius = 500;
                    Geocircle geocircle = new Geocircle(position, radius);
                    BasicGeoposition sloterdijkPosition;
                    sloterdijkPosition.Latitude = 52.389032;
                    sloterdijkPosition.Longitude = 4.821807;
                    sloterdijkPosition.Altitude = 0.0;
                    Geocircle geocircleSloterdijk = new Geocircle(sloterdijkPosition, radius);
                    BasicGeoposition hoornPosition;
                    hoornPosition.Latitude = 52.645247;
                    hoornPosition.Longitude = 5.054199;
                    hoornPosition.Altitude = 0.0;
                    Geocircle geocircleHoorn = new Geocircle(hoornPosition, radius);
                    bool singleUse = false;
                    MonitoredGeofenceStates mask = MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Exited | MonitoredGeofenceStates.Removed;
                    TimeSpan dwellTime = TimeSpan.FromSeconds(5);
                    TimeSpan duration = TimeSpan.FromHours(4);
                    DateTimeOffset startTime = DateTime.Now;
                    try
                    {
                        Geofence geofence = new Geofence(fenceId, geocircle, mask, singleUse, dwellTime, startTime, duration);
                        Geofence geofenceSloterdijk = new Geofence("sloterdijk", geocircleSloterdijk, mask, singleUse, dwellTime, startTime, duration);
                        Geofence geofenceHoorn = new Geofence("Hoorn", geocircleHoorn, mask, singleUse, dwellTime, startTime, duration);
                        geofences.Add(geofence);
                        geofences.Add(geofenceSloterdijk);
                        geofences.Add(geofenceHoorn);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e, "error");
                    }
                    RegisterBackgroundTask();
                    geofencesListView.ItemsSource = geofences;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e, "error");
                }
                foreach (var fence in GeofenceMonitor.Current.Geofences)
                {
                    Debug.WriteLine(fence, "fence");
                }
                Debug.WriteLine("test");
            }
        }

        async private void RegisterBackgroundTask()
        {
            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();

            geofenceTaskBuilder.Name = "geofenceTaskBuilderName";
            geofenceTaskBuilder.TaskEntryPoint = "BackgroundTask.GeofenceBackgroundTask";

            geofenceTaskBuilder.SetTrigger(new LocationTrigger(LocationTriggerType.Geofence));
            //geofenceTaskBuilder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));

            geofenceTask = geofenceTaskBuilder.Register();
            Debug.WriteLine("Backgroundtask registered");
        }
    }
}
