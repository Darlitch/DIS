using System.Xml.Serialization;
using Contract.Messaging;
using Contract.Xml;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker.Options;
using Worker.Services;

namespace Worker.Worker;

public class TaskConsumerService(BruteForceService bruteForceService, CallbackService callbackService,
    IOptions<RabbitMqOptions> rabbitOptions, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly RabbitMqOptions _options = rabbitOptions.Value;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };
        
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: MessagingTopology.TaskExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: MessagingTopology.TaskQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: MessagingTopology.TaskQueue,
            exchange: MessagingTopology.TaskExchange,
            routingKey: MessagingTopology.TaskRoutingKey,
            arguments: null,
            cancellationToken: stoppingToken);
        
        await channel.BasicQosAsync(0, 1, false, stoppingToken);
        
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

                await callbackService.SendResultAsync(response);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: MessagingTopology.TaskQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}