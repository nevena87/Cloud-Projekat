using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("Notification Service is running");

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

            // Create internal endpoint to check health status
            var internalEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["health-monitoring"];
            var internalListener = new TcpListener(internalEndpoint.IPEndpoint);
            internalListener.Start();
            Task.Run(() => ProccessRequests(internalListener));
            Trace.TraceInformation("Notification Service has been started...");

            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Notification Service is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Notification Service has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Notification Service is checking for emails...");
                await OnlineStatusChecker.IsCommentAvailableForSend();
                await OnlineStatusChecker.IsServiceOffline();
                await Task.Delay(10000);
            }
        }

        private async Task ProccessRequests(TcpListener listener)
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                using (var writer = new StreamWriter(client.GetStream()))
                {
                    await writer.WriteLineAsync("alive");
                    writer.Flush();
                }
                client.Close();
            }
        }
    }
}

