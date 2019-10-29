using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis.ClientLibrary;
using Microsoft.Extensions.Hosting;

namespace Demo.Kcl.Consumer
{
    public class MessageProcessorService : IHostedService
    {
        private KclProcess kclProcess;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            kclProcess = KclProcess.Create(new DataMessageProcessor());
            kclProcess.Run();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
