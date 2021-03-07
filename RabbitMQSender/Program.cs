using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQSender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type [quit] to exit");
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
                //不设置用户名和密码则使用默认guest/guest
            };

            using(var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    // 备胎交换机在消息找不到匹配队列时使用
                    // 不能被匹配的队列会被转移到备胎交换机/备胎队列

                    //定义备胎交换机
                    channel.ExchangeDeclare("alternate_exchange", "fanout", true, false, null);

                    //定义备胎队列
                    channel.QueueDeclare("alternate_queue", true, false, false, null);

                    //绑定备胎交换机和备胎队列
                    channel.QueueBind("alternate_queue", "alternate_exchange", "");

                    //设置备胎交换器参数
                    IDictionary<string,object> arguments = new Dictionary<string, object>();
                    arguments.Add("alternate-exchange", "alternate_exchange");

                    channel.ExchangeDeclare("rabbitmq_exchange", "direct", true, false, arguments); // arguments 启用备份交换机

                    // 定义死信交换机
                    channel.ExchangeDeclare("dead_exchange", "direct", true, false, null);

                    // 设置队列中消息的过期时间 （通过队列设置）
                    IDictionary<string, object> queueArgs = new Dictionary<string, object>();
                    queueArgs.Add("x-message-ttl", 20000);
                    // 指定转到死信队列的routingkey
                    queueArgs.Add("x-dead-letter-routing-key", "dead");
                    // 设置队列的死信交换机
                    queueArgs.Add("x-dead-letter-exchange", "dead_exchange");

                    // 定义死信队列
                    channel.QueueDeclare("dead_queue", true, false, false, null);

                    channel.QueueBind("dead_queue", "dead_exchange", "dead");

                    channel.QueueDeclare(queue: "debug_queue", true, false, false, queueArgs);
                    channel.QueueDeclare(queue: "error_queue", true, false, false, queueArgs);

                    channel.QueueBind("debug_queue", "rabbitmq_exchange", "debug", null); //bindingkey 为 debug
                    channel.QueueBind("error_queue", "rabbitmq_exchange", "error", null); //bindingkey 为 error

                    var properties = channel.CreateBasicProperties();
                    //RabbitMQ 服务器重启后是否保存消息，1 - 不保存，2 - 保存。
                    //注意：要让设置为2的时候起作用，必须在QueueDeclare的时候将durable设置为true
                    properties.DeliveryMode = 2; 
                    //properties.Expiration = "10000"; //设置消息的过期时间（通过消息设置）
                    while (true)
                    {
                        Console.Write(">>>routingkey message: ");
                        var messages = Console.ReadLine().Trim();
                        if (messages == "quit")
                        {
                            break;
                        }
                        var messageAry = messages.Split(" ");

                        //判断routingkey，routingkey 和bindingkey 匹配时，消息才能被路由到对应queue
                        if(messageAry[0] == "debug" || messageAry[0] == "error")
                        {
                            Console.WriteLine("Correct routing key...");
                        }
                        else
                        {
                            Console.WriteLine("Wrong routing key!!! Message may not be routed to queue");
                        }

                        var body = Encoding.UTF8.GetBytes(messageAry[1]);
                        channel.BasicPublish("rabbitmq_exchange", messageAry[0], basicProperties: properties, body: body);
                        Console.WriteLine($"[x] Sent {messageAry[1]}");

                    }
               
                }
            }
       
        }
    }
}
