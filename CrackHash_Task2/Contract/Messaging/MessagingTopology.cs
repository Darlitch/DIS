namespace Contract.Messaging;

public static class MessagingTopology
{
    public const string TaskExchange = "crackhash.tasks.exchange";
    public const string TaskQueue = "crackhash.tasks.queue";
    public const string TaskRoutingKey = "crackhash.task";

    public const string ResultExchange = "crackhash.results.exchange";
    public const string ResultQueue = "crackhash.results.queue";
    public const string ResultRoutingKey = "crackhash.result";
}