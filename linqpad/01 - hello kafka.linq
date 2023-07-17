<Query Kind="Program">
  <NuGetReference>Confluent.Kafka</NuGetReference>
  <NuGetReference>Polly</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>Polly</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var connection = "localhost:9092";
    var topic = $"test-topic-{Guid.NewGuid()}";
    int producerCount = 5;
	int messagesPerProducer = 5;// breaks at big number
    // Tasks for producers. 
    var producerTasks = Enumerable.Range(0, producerCount).Select(i => Task.Run(() => ProduceMessage(i, messagesPerProducer))).ToArray();

    // Tasks for consumers
    var consumerTask = Task.Run(async () =>
    {
        var retryPolicy = Policy
				.Handle<ConsumeException>()
				.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (e, x) => Console.WriteLine($"retrying: {e.Message}"));

        // Use Polly to retry subscribing to the topic if it initially fails
        await retryPolicy.ExecuteAsync(async () => await ConsumeMessages(topic, connection));

    });

    // Wait for all tasks to complete
    await Task.WhenAll(producerTasks.Concat(new[] { consumerTask }));

	Console.WriteLine("done");
    async Task ProduceMessage(int i, int messagesToSend)
	{
		
		Console.WriteLine($"starting producer {i}");
        var config = new ProducerConfig
        {
            BootstrapServers = connection,
            ClientId = $"Producer{i}"
        };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
		{
			List<Task> tasks = new List<Task>();
			for (int j = 0; j < messagesToSend; j++)
			{
				var message = new Message<Null, string> { Value = $"Hello, Kafka! Message {i}:{j}" };
				tasks.Add(producer.ProduceAsync(topic, message));
			}
			await Task.WhenAll(tasks);

		}
		
		Console.WriteLine($"closing producer {i}");
    }

    async Task ConsumeMessages(string topic, string connection)
    {
		Console.WriteLine("starting consumer");
        var consumerConfig = new ConsumerConfig
        {
			
            BootstrapServers = connection,
            GroupId = $"ConsumerGroup{Guid.NewGuid()}", // Unique consumer group id for each run
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
        {
			consumer.Subscribe(topic);
            while (true)
            {
                var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1)); // Specify a timeout to prevent blocking indefinitely
                if (consumeResult != null)
				{
					Console.WriteLine($"Received message: {consumeResult.Message.Value}");
				}
				else
				{
					// Exit the loop if no message is received within the specified timeout
					break;
				}
			}
		}
	}
}