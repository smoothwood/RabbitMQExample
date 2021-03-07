using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCClient
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
                    List<string> correlationIdList = new List<string>();
                    channel.ExchangeDeclare("rpc_exchange", "direct", true, false, null);

                    string queueName = channel.QueueDeclare().QueueName;

                    channel.QueueBind(queueName, "rpc_exchange", queueName, null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, e)=>
                    {
                        string message = Encoding.UTF8.GetString(e.Body.ToArray());
                        string correlationId = e.BasicProperties.CorrelationId;
                        if (correlationIdList.IndexOf(correlationId) > -1)
                        {
                            Console.WriteLine($"Found CorrelationId: {correlationId}");
                            correlationIdList.Remove(correlationId);
                        }
                        Console.WriteLine($"Client Received Message: {message}");
                        Console.WriteLine($"Client Received CorrelationId from Server: {correlationId}");
                    };
                    channel.BasicConsume(queueName, true, consumer);

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.ReplyTo = queueName;

                    while (true)
                    {
                        Console.Write("Input message: ");
                        string message = Console.ReadLine();
                        string correlationId = Guid.NewGuid().ToString();
                        properties.CorrelationId = correlationId;
                        correlationIdList.Add(correlationId);
                        channel.BasicPublish("rpc_exchange", "rpc_routing_key", properties, Encoding.UTF8.GetBytes(message));
                    }
                    
                }
            }


        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            
        }
    }
}
