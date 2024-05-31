using KorisnikService_Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HealthStatusService.Controllers
{
    public class HealthStatusController : Controller
    {
        public async Task<ActionResult> Index()
        {
            (double uptime, List<bool> stats) = await RetreiveData();
            ViewBag.uptime = uptime;
            ViewBag.uptimePercentage = Math.Round(uptime, 0).ToString() + "%";
            ViewBag.uptimeClass = GetUptimeClass(uptime);
            ViewBag.stats = stats;
            return View();
        }

        private string GetUptimeClass(double uptime)
        {
            if (uptime > 90)
                return "bg-success text-success";
            else if (uptime >= 80)
                return "bg-warning text-warning";
            else
                return "bg-danger text-danger";
        }

        public async static Task<(double, List<bool>)> RetreiveData()
        {
            double uptime = 0.0;
            List<bool> statuses = new List<bool>();

            try
            {
                List<KorisnikService_Data.HealthStatus> stats = await new HealthCheckRepository().ReadStatusesAsync();
                DateTime now = DateTime.Now;
                DateTime hourBefore = now.AddHours(-1);

                List<KorisnikService_Data.HealthStatus> withinLastHour = stats.FindAll(s => s.Timestamp >= hourBefore && s.Timestamp <= now);
                statuses = withinLastHour.Select(x => x.IsRedditAvailable).ToList();
                statuses = ResampleData(statuses, 215);

                // Calculate percentage in last 24h
                int online = stats.Count(x => x.IsRedditAvailable == true && x.Timestamp >= now.AddHours(-24));
                int offline = stats.Count(x => x.IsRedditAvailable == false && x.Timestamp >= now.AddHours(-24));

                uptime = Math.Round(((1.0 * online) / (offline + online) * 100), 3);

                return (uptime, statuses);
            }
            catch (Exception)
            {
                return (0.0, new List<bool>());
            }
        }

        public static List<bool> ResampleData(List<bool> data, int requested)
        {
            List<bool> resampled = new List<bool>();

            // Return an empty list if the data list is null, empty, or the target count is invalid
            if (data == null || data.Count == 0 || requested <= 0)
                return resampled;

            // If the data list has the same count as the target count, return it as is
            if (data.Count == requested)
                return new List<bool>(data);

            // Calculate the step size for resampling
            double step = (double)(data.Count - 1) / (requested - 1);

            // Resample the data
            for (int i = 0; i < requested; i++)
            {
                double index = i * step;
                int lowerIndex = (int)index;
                int upperIndex = Math.Min(lowerIndex + 1, data.Count - 1);

                // Handle out-of-range indices
                if (lowerIndex < 0 || upperIndex < 0 || lowerIndex >= data.Count || upperIndex >= data.Count)
                    resampled.Add(false); // Assuming false for out-of-range indices
                else
                    resampled.Add(data[lowerIndex] && data[upperIndex]); // Use logical AND for boolean values
            }

            return resampled;
        }
    }
}