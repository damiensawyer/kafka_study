<Query Kind="Program">
  <NuGetReference>Confluent.Kafka</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var connection = "localhost:9092";
    var topic = "test-topic";
    int messageCount = 20;

    // Tasks for producers
    var producerTasks = Enumerable.Range(0, messageCount).Select(i => Task.Run(() => ProduceMessage(i))).ToArray();

    // Tasks for consumers
    var consumerTasks = Enumerable.Range(0, messageCount).Select(i => Task.Run(() => ConsumeMessage())).ToArray();

    // Wait for all tasks to complete
    await Task.WhenAll(producerTasks.Concat(consumerTasks));

    async Task ProduceMessage(int i)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = connection, 
            ClientId = $"Producer{i}"
        };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            var message = new Message<Null, string> { Value = $"Hello, Kafka! Message {i}" };
            await producer.ProduceAsync(topic, message);
        }
    }

    async Task ConsumeMessage()
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = connection,
            GroupId = $"ConsumerGroup{Guid.NewGuid()}", // Unique consumer group id for each run
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
        {
            consumer.Subscribe(topic);
			var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1)); // Specify a timeout to prevent blocking indefinitely
			if (consumeResult != null)
			{
				Console.WriteLine($"Received message: {consumeResult.Message.Value}");
			}
		}
	}
}
