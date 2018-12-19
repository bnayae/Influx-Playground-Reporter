using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Collector;

namespace Bnaya.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var lifetime = ContainerHosting.WhileRunning();
            while (!lifetime.IsCompleted)
            {
                try
                {
                    string endpoint = Environment.GetEnvironmentVariable("endpoints.influxdb.dns");
                    if (string.IsNullOrEmpty(endpoint))
                        endpoint = "influx-influxdb";
                    string port = Environment.GetEnvironmentVariable("endpoints.influxdb.port");
                    if (string.IsNullOrEmpty(port))
                        port = "8086";

                    Metrics.Collector = new CollectorConfiguration()
                        .Batch.AtInterval(TimeSpan.FromSeconds(2))
                        .Tag.With("host", Environment.MachineName)
                        .Tag.With("user", Environment.UserName)
                        .Tag.With("App", "influx-reporter-csharp")
                        .WriteTo.InfluxDB($"http://{endpoint}:{port}", "telegraf")
                        .CreateCollector();

                    Metrics.Increment("connecting");
                    break;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Format(ErrorFormattingOption.FormatDuplication | ErrorFormattingOption.IncludeLineNumber));
                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }

            var process = Process.GetCurrentProcess();
            while (!lifetime.IsCompleted)
            {
                Metrics.Increment("net_iterations");

                Metrics.Write("net_cpu_time",
                    new Dictionary<string, object>
                    {
                        { "value", process.TotalProcessorTime.TotalMilliseconds },
                        { "user", process.UserProcessorTime.TotalMilliseconds }
                    });

                int minuete = DateTime.Now.Minute;
                var sw = Stopwatch.StartNew();
                int i = 0;
                using (Metrics.Time("carzy_qlock_time"))
                {
                    while (sw.Elapsed.TotalSeconds < minuete)
                    {
                        if (i++ % 1_000_000 == 0)
                            Metrics.Increment("crazy_time_count");

                        // use CPU
                    }
                }
                int delay = rnd.Next(20, 3000);
                Metrics.Increment("crazy_rnd_level", delay);
                using (Metrics.Time("carzy_rnd_time"))
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                }
            }
        }
    }
}
