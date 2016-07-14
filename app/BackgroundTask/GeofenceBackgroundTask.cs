using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    public sealed class GeofenceBackgroundTask : IBackgroundTask
    {
        void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Backgroundtask activated");
            try
            {
                // Handle geofence state change reports
                GetGeofenceStateChangedReports();
            }
            catch (UnauthorizedAccessException)
            {
                DoToast("Unautorized");
            }
        }

        private void GetGeofenceStateChangedReports()
        {
            GeofenceMonitor monitor = GeofenceMonitor.Current;
            String geofenceItemEvent = null;

            var reports = GeofenceMonitor.Current.ReadReports();
            foreach (var report in reports)
            {
                GeofenceState state = report.NewState;
                geofenceItemEvent = report.Geofence.Id;
                if (state == GeofenceState.Removed)
                {
                    GeofenceRemovalReason reason = report.RemovalReason;
                    if (reason == GeofenceRemovalReason.Expired)
                    {
                        geofenceItemEvent += " (Removed/Expired)";
                        Debug.WriteLine(report.Geofence.Id + " Removed/Expired");
                    }
                    else if (reason == GeofenceRemovalReason.Used)
                    {
                        geofenceItemEvent += " (Removed/Used)";
                        Debug.WriteLine(report.Geofence.Id + " Removed/Used");
                    }
                }
                if (state == GeofenceState.Entered)
                {
                    geofenceItemEvent += " (Entered)";
                    Debug.WriteLine(report.Geofence.Id + " Entered");
                }
                if (state == GeofenceState.Exited)
                {
                    geofenceItemEvent += " (Exited)";
                    Debug.WriteLine(report.Geofence.Id + " Exited");
                }
                else
                {
                    Debug.WriteLine(report.Geofence.Id, " Geen state veranderd");
                }
            }
            DoToast(geofenceItemEvent);
        }

        private void DoToast(String eventName)
        {
            // pop a toast for each geofence event
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();

            // Create a two line toast and add audio reminder

            // Here the xml that will be passed to the 
            // ToastNotification for the toast is retrieved
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            // Set both lines of text
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode("Geolocation GeofenceBackgroundTask"));
            if (!String.IsNullOrEmpty(eventName))
            {
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(eventName));
            }
            String secondLine = "geofenceBackgroundTask";
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
