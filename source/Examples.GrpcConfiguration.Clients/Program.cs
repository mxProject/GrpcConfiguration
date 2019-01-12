using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Grpc.Core;
using Grpc.Core.Interceptors;
using mxProject.Helpers.Grpc.Configuration;
using mxProject.Helpers.Grpc.Configuration.Interceptors;
using mxProject.Helpers.Grpc.Configuration.KeyValues;
using Examples.GrpcConfiguration.Models;

namespace Examples.GrpcConfiguration.Clients
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            KeyValueConfigBase keyValue = new StringValueConfig() { Name = "a", Value = "1" };
            XmlSerializer s = new XmlSerializer(typeof(KeyValueConfigBase));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                s.Serialize(ms, keyValue);
                System.Diagnostics.Debug.WriteLine(System.Text.Encoding.Default.GetString(ms.ToArray()));
            }

            // Load the config from the file.
            RpcConfigurationConfig config = LoadConfig(@".\GrpcClient.config");

            // Create the context.
            using (RpcConfigurationContext context = new RpcConfigurationContext(config, true))
            {

                Application.Run(new Form1(context));

            }

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
