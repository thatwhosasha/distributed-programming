using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    private const string ExchangeName = "valuator.processing.rank";
    private const string QueueName = "valuator.processing.rank";

    private static IDatabase _db = null!;

    public static async Task Main(string[] args)
    {
        Console.WriteLine($"[PID {Environment.ProcessId}] RankCalculator started");

        // Redis
        var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
        var redis = await ConnectionMultiplexer.ConnectAsync(redisConn);
        _db = redis.GetDatabase();

        // RabbitMQ
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };
        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel);

        // По одному сообщению за раз — честное распределение между экземплярами
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        string consumerTag = await RunConsumer(channel);

        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();

        await channel.BasicCancelAsync(consumerTag);
    }

    private static async Task<string> RunConsumer(IChannel channel)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ConsumeAsync(channel, eventArgs);
        return await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );
    }

    private static async Task ConsumeAsync(IChannel channel, BasicDeliverEventArgs eventArgs)
    {
        string json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        Console.WriteLine($"[PID {Environment.ProcessId}] Consuming: {json}");

        var message = JsonSerializer.Deserialize<RankMessage>(json);
        if (message is null)
        {
            await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            return;
        }

        double rank = ComputeRank(message.Text);

        await _db.StringSetAsync($"rank:{message.Id}", rank);
        Console.WriteLine($"[PID {Environment.ProcessId}] rank[{message.Id}] = {rank}");

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
    }

    private static double ComputeRank(string text)
    {
        // Перенесите сюда логику вычисления из Valuator (PA1)
        // Пример-заглушка:
        return text.Length;
    }

    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct
        );
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: ""
        );
    }

    private const string EventsExchange = "events";

    private static async Task PublishRankCalculatedAsync(
        IChannel channel, string id, double rank)
    {
        await channel.ExchangeDeclareAsync(
            exchange: EventsExchange,
            type: ExchangeType.Topic);

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Rank = rank
        });

        var body = Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            exchange: EventsExchange,
            routingKey: "rank.calculated",
            mandatory: false,
            body: body);
    }
}

record RankMessage(string Id, string Text);

