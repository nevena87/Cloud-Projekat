using Microsoft.WindowsAzure.ServiceRuntime;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HealthMonitoringService.HealthCheck
{
    public class NotificationServiceHC
    {
        public static async Task<bool> RunCheck()
        {
            bool is_online = false;

            try
            {
                var endpoint = RoleEnvironment.Roles["NotificationService"].Instances.First().InstanceEndpoints["health-monitoring"];
                string ipAddress = endpoint.IPEndpoint.Address.ToString();
                int port = endpoint.IPEndpoint.Port;

                using (var client = new TcpClient(ipAddress, port))
                {
                    using (var reader = new StreamReader(client.GetStream()))
                    {
                        string response = await reader.ReadLineAsync();
                        is_online = true;
                    }
                }
            }
            catch
            {
                is_online = false;
            }

            return is_online;
        }
    }
}
