using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RedditService
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // Create internal endpoint to check health status
            var internalEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["health-monitoring-r"];
            var internalListener = new TcpListener(internalEndpoint.IPEndpoint);
            internalListener.Start();
            Task.Run(() => ProccessRequests(internalListener));
            Trace.TraceInformation("Notification Service has been started...");

            return base.OnStart();
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
