using Dummy;
using Greet;
using Grpc.Core;
using Sqrt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";
        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
             {
                 if (task.Status == TaskStatus.RanToCompletion)
                 {
                     Console.WriteLine("the client connected");
                 }
             });

            //var client = new GreetingService.GreetingServiceClient(channel);
            //
            //var greeting = new Greeting()
            //{
            //    FirstName = "first",
            //    LastName = "last"
            //};

            //Unary(greeting, client);
            //await ServerStreaming(greeting, client);
            //await ClientStreaming(greeting, client);
            //await BiDiStreaming(greeting, client);

            var client = new SqrtService.SqrtServiceClient(channel);
            int number = -1;

            try
            {
                var response = client.sqrt(new SqrtRequest() { Number = number });
                Console.WriteLine(response.SquareRoot);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }



            channel.ShutdownAsync().Wait();

            Console.ReadKey();
        }

        private static void Unary(Greeting greeting, GreetingService.GreetingServiceClient client)
        {
            var request = new GreetingRequest() { Greeting = greeting };
            var response = client.Greet(request);

            Console.WriteLine(response.Result);
        }
        private static async Task ServerStreaming(Greeting greeting, GreetingService.GreetingServiceClient client)
        {
            var request = new GreetManyTimesRequest() { Greeting = greeting };
            var response = client.GreetManyTimes(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }

        }

        private static async Task ClientStreaming(Greeting greeting, GreetingService.GreetingServiceClient client)
        {
            var request = new LongGreetRequest() { Greeting = greeting };
            var stream = client.LongGreet();

            foreach (int i in Enumerable.Range(1, 10))
            {
                await stream.RequestStream.WriteAsync(request);
            }

            await stream.RequestStream.CompleteAsync();
            var response = stream.ResponseAsync;
            Console.WriteLine(response.Result);
        }

        private static async Task BiDiStreaming(Greeting greeting, GreetingService.GreetingServiceClient client)
        {
            var stream = client.GreetEveryone();
            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("received - " + stream.ResponseStream.Current.Result);
                }
            });

            Greeting[] greetings =
            {
                new Greeting() { FirstName = "123", LastName = "234"},
                new Greeting() { FirstName = "first", LastName = "last"},
                new Greeting() { FirstName = "name", LastName = "nana"},
            };

            foreach (var item in greetings)
            {
                Console.WriteLine("sending - " + item.ToString());
                await stream.RequestStream.WriteAsync(new GreetEveryoneRequest()
                {
                    Greeting = item
                });
                await Task.Delay(100);
            }

            await stream.RequestStream.CompleteAsync();
            await responseReaderTask;
        }
    }
}
