using System.Xml.Serialization;
using Contract.Messaging;
using Contract.Xml;
using Manager.Options;
using Manager.Repositories;
using Manager.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Manager.BackgroundServices;

public class ResultConsumerService(HashCrackService hashCrackService, IOptions<RabbitMqOptions> rabbitOptions) : BackgroundService
{
    private readonly RabbitMqOptions _options = rabbitOptions.Value;
    
    protected override async Task ExecuteAsync(CancellationToken ct)
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
            exchange: MessagingTopology.ResultExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: MessagingTopology.ResultQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: MessagingTopology.ResultQueue,
            exchange: MessagingTopology.ResultExchange,
            routingKey: MessagingTopology.ResultRoutingKey,
            arguments: null,
            cancellationToken: ct);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var serializer = new XmlSerializer(typeof(WorkerTaskResponse));
                using var stream = new MemoryStream(ea.Body.ToArray());
                var response = (WorkerTaskResponse)serializer.Deserialize(stream)!;

                await hashCrackService.ProcessWorkerResult(response, ct);
                await channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, ct);
            }
        };
        
        await channel.BasicConsumeAsync(
            queue: MessagingTopology.ResultQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        await Task.Delay(Timeout.Infinite, ct);
    }
}