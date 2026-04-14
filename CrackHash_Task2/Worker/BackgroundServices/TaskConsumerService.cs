using System.Xml.Serialization;
using Contract.Messaging;
using Contract.Xml;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Options;
using Worker.Services;

namespace Worker.BackgroundServices;

public class TaskConsumerService(BruteForceService bruteForceService, ResultPublisher resultPublisher,
    IOptions<RabbitMqOptions> rabbitOptions) : BackgroundService
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
        
        await channel.BasicQosAsync(0, 1, false, ct);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var serializer = new XmlSerializer(typeof(WorkerTaskRequest));
                using var stream = new MemoryStream(ea.Body.ToArray());
                var request = (WorkerTaskRequest)serializer.Deserialize(stream)!;
                
                var answers = bruteForceService.FindMatches(request);

                var response = new WorkerTaskResponse
                {
                    RequestId = request.RequestId,
                    PartNumber = request.PartNumber,
                    Answers = new Answers { Words = answers }
                };

                await resultPublisher.PublishAsync(response, ct);
                await channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, ct);
            }
        };

        await channel.BasicConsumeAsync(
            queue: MessagingTopology.TaskQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        await Task.Delay(Timeout.Infinite, ct);
    }
}