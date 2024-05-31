using HealthMonitoringService.HealthCheck;
using KorisnikService_Data;
using KorisnikService_Data.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HealthStatus = KorisnikService_Data.HealthStatus;

namespace HealthMonitoringService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;
            bool result = base.OnStart();
            Trace.TraceInformation("HealthMonitoringService has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("HealthMonitoringService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Checking health...");

                bool notification = await NotificationServiceHC.RunCheck();
                bool reddit = await RedditHC.RunCheck();

                HealthStatus status = new HealthStatus(reddit, notification);
                await new HealthCheckRepository().InsertStatusAsync(status);

                // Create a message for email
                string emailBody = "";

                if (!reddit) emailBody += "Reddit is offline.\n";
                if (!notification) emailBody += "Notification is offline.\n";

                // Add email into queue
                _ = new CloudQueueHelper(); // singleton instance
                if (emailBody != "") await CloudQueueHelper.AddToQueue(CloudQueueHelper.GetQueue("AdminNotificationQueue"), emailBody);

                await Task.Delay(5000);
            }
        }
    }
}
