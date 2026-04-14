using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Contract.Messaging;
using Contract.Xml;
using Manager.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Manager.Services;

public class TaskPublisher(IOptions<RabbitMqOptions> rabbitOptions)
{
    private readonly RabbitMqOptions _options = rabbitOptions.Value;
    
    public async Task PublishAsync(WorkerTaskRequest request, CancellationToken ct = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };
        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        
        await channel.ExchangeDeclareAsync(
            exchange: MessagingTopology.TaskExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);
        
        await channel.QueueDeclareAsync(
            queue: MessagingTopology.TaskQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);
        
        await channel.QueueBindAsync(
            queue: MessagingTopology.TaskQueue,
            exchange: MessagingTopology.TaskExchange,
            routingKey: MessagingTopology.TaskRoutingKey,
            arguments: null,
            cancellationToken: ct);
        
        var serializer = new XmlSerializer(typeof(WorkerTaskRequest));
        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, request);
        }
        var body = stream.ToArray();
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/xml"
        };
        
        await channel.BasicPublishAsync(
            exchange: MessagingTopology.TaskExchange,
            routingKey: MessagingTopology.TaskRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }
}