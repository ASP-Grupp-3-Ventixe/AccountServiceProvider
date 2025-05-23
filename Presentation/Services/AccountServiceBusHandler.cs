//using Azure.Messaging.ServiceBus;

//namespace Presentation.Services;

//public class AccountServiceBusHandler
//{
//    private readonly ServiceBusClient _client;
//    private readonly ServiceBusSender _sender;

//    public AccountServiceBusHandler(IConfiguration configuration)
//    {
//        _client = new ServiceBusClient(configuration["ServiceBus:ConnectionString"]);
//        _sender = _client.CreateSender(configuration["ServiceBus:QueueName"]);  
//    }

//    public async Task PublishAsync(string payload)
//    {
//        var message = new ServiceBusMessage(payload);

//        await _sender.SendMessageAsync(message);
//        await _sender.DisposeAsync();
//        await _client.DisposeAsync();
//    }
//}
