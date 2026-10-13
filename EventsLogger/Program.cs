using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger;

class Program
{
    private const string ExchangeName = "events";

    public static async Task Main(string[] args)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Exchange типа topic для событий
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic);

        // Эксклюзивная очередь (auto-delete, генерируется имя) — у каждого экземпляра своя.
        // Это даёт настоящий pub/sub: каждый экземпляр получает все события.
        var queue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true);

        // Подписываемся на оба события: *.calculated
        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: ExchangeName,
            routingKey: "*.calculated");

        Console.WriteLine($"[PID {Environment.ProcessId}] EventsLogger started. Queue={queue.QueueName}");
        Console.WriteLine("Подписка: rank.calculated, similarity.calculated");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) =>
        {
            var routingKey = ea.RoutingKey;             // rank.calculated / similarity.calculated
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"[PID {Environment.ProcessId}] EVENT '{routingKey}': {body}");
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer: consumer);

        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();
    }
}