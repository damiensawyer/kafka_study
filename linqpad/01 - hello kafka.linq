<Query Kind="Program">
  <NuGetReference>Confluent.Kafka</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
</Query>

void Main()
{
    // Kafka server configuration
    var config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092", // Replace with your Kafka server address
        ClientId = "LINQPadProducer"
    };

    // Create a topic
    var topic = "test-topic";

    using (var producer = new ProducerBuilder<Null, string>(config).Build())
    {
        // Publish a message to the topic
        var message = new Message<Null, string> { Value = "Hello, Kafka!" };
        producer.ProduceAsync(topic, message).Wait();
        producer.Flush(TimeSpan.FromSeconds(10));
    }

    // Read the message from another service
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092", // Replace with your Kafka server address
        GroupId = "LINQPadConsumer",
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
    {
        consumer.Subscribe(topic);AcceptRejectRule:

        while (true)
        {
            var consumeResult = consumer.Consume();
            Console.WriteLine($"Received message: {consumeResult.Message.Value}");
        }
    }
}