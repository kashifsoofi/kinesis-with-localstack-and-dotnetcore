namespace Consumer.Host
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    class Program
    {
        static async Task Main()
        {
            Console.Title = "Consumer.Host";

            var host = new HostBuilder()
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    services.AddHostedService<NServiceBusService>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}