using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQReceiver
{
    class Program
    {
        static IConnection connection;
        static IModel channel;
        static void Main(string[] args)
        {
            Console.Write("Input Queue Name: ");
            var queueName = Console.ReadLine().Trim();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            connection = factory.CreateConnection();

            channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName,
                        true, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(queue: queueName, false, consumer);
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
                    
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[x] Received {message}");
            channel.BasicAck(e.DeliveryTag, false);
        }
    }
}
