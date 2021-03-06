using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                //HostName = "localhost"
                UserName = "ycm",
                Password = "ycm119"

            };



            using (var connection = factory.CreateConnection(new string[2] { "52.246.181.64", "40.115.183.202" }))
            {
                using(var channel = connection.CreateModel())
                {
                    
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += Consumer_Received;
                    channel.BasicConsume("normal_queue", true, consumer);
                    Console.ReadLine();
                }
            }
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"[x] Received {message}");
        }
    }
}
