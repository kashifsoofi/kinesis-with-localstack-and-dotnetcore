namespace Consumer.Host
{
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime.SharedInterfaces;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;

    public class SnsService
    {
        private readonly AmazonSimpleNotificationServiceClient client;

        public SnsService()
        {
            var serverName = "localhost";
            client = new AmazonSimpleNotificationServiceClient(
                "DUMMY_ACCESS_KEY_ID",
                "DUMMY_SECRET_ACCESS_KEY",
                new AmazonSimpleNotificationServiceConfig
                {
                    ServiceURL = $"http://{serverName}:4575",
                    // RegionEndpoint = RegionEndpoint.GetBySystemName("eu-west-1"),
                });
        }

        public async Task<string> GetOrCreateTopicArn(string topicName)
        {
            var topic = await client.FindTopicAsync(topicName);
            if (topic != null)
            {
                return topic.TopicArn;
            }

            var createTopicResponse = await client.CreateTopicAsync(new CreateTopicRequest
            {
                Name = topicName,                
            });
            return createTopicResponse.TopicArn;
        }

        public async Task<string> PublishAsync(string topicArn, string message)
        {
            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topicArn,
            };
            var response = await client.PublishAsync(request);
            return response.MessageId;
        }

        public async Task<string> SubscribeAsync(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
        {
            var listSubscriptionsByTopicResponse = await client.ListSubscriptionsByTopicAsync(topicArn);
            var sqsQueueSubscription = listSubscriptionsByTopicResponse.Subscriptions.FirstOrDefault(x => x.Endpoint == sqsQueueUrl);
            if (sqsQueueSubscription != null)
            {
                return sqsQueueSubscription.SubscriptionArn;
            }

            var result = await client.SubscribeQueueAsync(topicArn, sqsClient, sqsQueueUrl);
            return result;
        }
    }
}
