using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Consumer
{
    class Program
    {
        static async Task Main()
        {
            // Wait for stream to become available
            Thread.Sleep(15000);

            var host = new HostBuilder()
                .ConfigureServices((services) =>
                {
                    services.AddHostedService<MessageConsumerService>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}
