using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Grpc.Core;
using Grpc.Core.Interceptors;
using mxProject.Helpers.Grpc.Configuration;
using mxProject.Helpers.Grpc.Configuration.Interceptors;
using Examples.GrpcConfiguration.Models;

namespace Examples.GrpcConfiguration.Servers
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Load the config from the file.
            RpcConfigurationConfig config = LoadConfig(@".\GrpcServer.config");

            // Create the context.
            using (RpcConfigurationContext context = new RpcConfigurationContext(config, true))
            {

                Server server = new Server();

                server.Services.Add(context.Intercept(ExampleService.BindService(new ExampleServiceImpl()), "example1"));

                server.Ports.Add(context.GetServerPort("channel1"));

                server.Start();

                Console.WriteLine("The server has started.");
                Console.WriteLine("If you press any key, this application will terminate.");
                Console.ReadLine();

            }

            Console.WriteLine("The server has shutdown.");
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Load the config from the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static RpcConfigurationConfig LoadConfig(string filePath)
        {
            // Specify the types included in the config.
            Type[] extraTypes =
            {
                typeof(ExampleInterceptorConfig)
            };

            XmlSerializer serializer = new XmlSerializer(typeof(RpcConfigurationConfig), extraTypes);

            RpcConfigurationConfig config = new RpcConfigurationConfig();
            
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                return (RpcConfigurationConfig)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Create a interceptor.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Interceptor CreateExampleInterceptor(CustomInterceptorConfig config)
        {
            if (config.TryGetMethodArgs("name", out object name))
            {
                return new ExampleInterceptor(name.ToString());
            }
            else
            {
                return new ExampleInterceptor();
            }
        }

    }
}
