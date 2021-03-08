using RabbitMQ.Client;
using System;
using System.Text;

namespace Send
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
            //using (var connection = factory.CreateConnection())
            using (var connection = factory.CreateConnection(new string[2] { "52.246.181.64", "40.115.183.202" }))
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("normal_exchange", "direct", true, false, null);
                    channel.QueueDeclare("normal_queue", true, false, false, null);
                    channel.QueueBind("normal_queue", "normal_exchange", "routing_key",null);
                    string message = "Hello World!";
            
                    channel.BasicPublish("normal_exchange", "routing_key", null, Encoding.UTF8.GetBytes(message));
                    Console.WriteLine($"[x] Sent {message}");
                    Console.ReadLine();
                }
            }
        }
    }
}
