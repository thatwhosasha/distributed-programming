using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Valuator.Services;

public class RabbitPublisher : IAsyncDisposable
{
    private const string ExchangeName = "valuator.processing.rank";
    private const string QueueName = "valuator.processing.rank";

    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitPublisher(IConfiguration config)
    {
        var host = config["RABBITMQ_HOST"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = host };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        DeclareTopologyAsync(_channel).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(string id, string text)
    {
        var payload = JsonSerializer.Serialize(new { Id = id, Text = text });
        var body = Encoding.UTF8.GetBytes(payload);

        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "",
            mandatory: false,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }

    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(QueueName, ExchangeName, routingKey: "");
    }

    public async Task PublishSimilarityCalculatedAsync(string id, double similarity)
    {
    await _channel.ExchangeDeclareAsync("events", ExchangeType.Topic);

    var payload = JsonSerializer.Serialize(new { Id = id, Similarity = similarity });
    var body = Encoding.UTF8.GetBytes(payload);

    await _channel.BasicPublishAsync(
        exchange: "events",
        routingKey: "similarity.calculated",
        mandatory: false,
        body: body);
    }
}