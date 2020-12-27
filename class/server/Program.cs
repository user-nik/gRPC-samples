using Greet;
using Grpc.Core;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using Sqrt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        const int Port = 50051;
        static void Main(string[] args)
        {
            Server server = null;

            try
            {
                var reflectionServiceImpl = new ReflectionServiceImpl(GreetingService.Descriptor, ServerReflection.Descriptor);
                server = new Server()
                {
                    Services = {
                        SqrtService.BindService(new SqrtServiceIml()),
                        GreetingService.BindService(new GreetingServiceImpl()),
                        ServerReflection.BindService(reflectionServiceImpl),
                    },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine($"listening on port {Port}");

                Console.ReadKey();
            }
            catch (IOException ex)
            {
                Console.WriteLine("failt to start " + ex.Message);
                throw;
            }
            finally
            {
                if(server != null)
                {
                    server.ShutdownAsync().Wait();
                }
            }
        }
    }
}
