using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RPCServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using(var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("rpc_exchange", "direct", true, false, null);
                    channel.QueueDeclare("rpc_queue", true, false, false, null);
                    channel.QueueBind("rpc_queue", "rpc_exchange", "rpc_routing_key", null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender,e)=>
                    {
                        string correlationId = e.BasicProperties.CorrelationId;
                        string replyToQueue = e.BasicProperties.ReplyTo;
                        string message = Encoding.UTF8.GetString(e.Body.ToArray());

                        Console.WriteLine($"[x] Server Received CorrelationId: {correlationId}");
                        Console.WriteLine($"[x] Server Received ReplyToQueue: {replyToQueue}");
                        Console.WriteLine($"[x] Server Received Message: {message}");

                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.CorrelationId = correlationId;
                        channel.BasicPublish("rpc_exchange", replyToQueue, properties, Encoding.UTF8.GetBytes(message + "!!!"));
                        channel.BasicAck(e.DeliveryTag, false);
                    };
                    channel.BasicConsume("rpc_queue", false, consumer);
                    Console.ReadLine();
                }
            }
        }


    }
}
