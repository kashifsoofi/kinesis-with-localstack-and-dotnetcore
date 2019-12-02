using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.KinesisEvents;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Demo.Consumer.Lambda
{

    public class Function
    {
        public async Task Handler(KinesisEvent kinesisEvent, ILambdaContext context)
        {
            foreach (var record in kinesisEvent.Records)
            {

            }

            await Task.CompletedTask;
        }
    }
}
