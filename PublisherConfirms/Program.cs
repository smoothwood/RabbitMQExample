using RabbitMQ.Client;
using System;
using System.Text;

namespace PublisherConfirms
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
                    channel.ExchangeDeclare("publisher_confirms_exchange", "fanout", true, false, null);
                    channel.QueueDeclare("publisher_confirms_queue", true, false, false, null);
                    channel.QueueBind("publisher_confirms_queue", "publisher_confirms_exchange", "", null);

                    channel.ConfirmSelect();
                    channel.BasicAcks += Channel_BasicAcks;
                    channel.BasicNacks += Channel_BasicNacks;

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    while (true)
                    {
                        string input = Console.ReadLine().Trim();
                        if (input == "exit")
                        {
                            break;
                        }
                        var seqNo = channel.NextPublishSeqNo;
                        Console.WriteLine($"Sequence Number:{seqNo}");
                        channel.BasicPublish("publisher_confirms_exchange", "", properties, Encoding.UTF8.GetBytes(input));
                    }
                }
            }
        }

        private static void Channel_BasicNacks(object sender, RabbitMQ.Client.Events.BasicNackEventArgs e)
        {
            //TODO
        }

        private static void Channel_BasicAcks(object sender, RabbitMQ.Client.Events.BasicAckEventArgs e)
        {
            Console.WriteLine($"Returned Sequence Number: {e.DeliveryTag}");
        }

    }
}
