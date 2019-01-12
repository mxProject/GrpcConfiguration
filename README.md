

# mxProject.Helpers.GrpcConfiguration

gRPC configuration library for .NET Framework and .NET Standard.

[Japanese page](README.jp.md)

## Features

* Generate the following objects from the the contents described in the XML file.
  * CallInvoker
  * Channel
  * ChannelCredentials
  * ServerPort
  * ServerCredentials
  * Interceptor

## Requrements

* .NET Framework >= 4.6
* .NET Standard >= 2.0
* Grpc.Core >= 1.16.0

## Licence

[MIT Licence](http://opensource.org/licenses/mit-license.php)

## Usase

### Service side configuration file

Serialize an instance of type `RpcConfigurationConfig` using XmlSerializer.

```xml:GrpcServer.config
<?xml version="1.0" encoding="utf-8"?>
<RpcConfigurationConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">

  <!-- Interceptors shared by multiple services -->
  <Interceptors>

    <!-- Example of specifying a method to generate an interceptor -->
    <Custom Name="interceptor1" TypeName="Examples.GrpcConfiguration.Servers.Program, Examples.GrpcConfiguration.Servers" MethodName="CreateExampleInterceptor">
      <MethodArgs>
        <String Name="name" Value="clientIntercept1" />
      </MethodArgs>
    </Custom>

    <!-- Example of specifying an interceptor type -->
    <Custom Name="interceptor2" TypeName="Examples.GrpcConfiguration.Models.ExampleInterceptor, Examples.GrpcConfiguration.Models" />

  </Interceptors>

  <ExtraInterceptors>

    <!-- Example of specifying a configuration corresponding to an interceptor -->
    <Extra xsi:type="ExampleInterceptorConfig" Name="interceptor3" InterceptorName="serverIntercept3" />

  </ExtraInterceptors>

  <!-- Credentials -->
  <Credentials>

    <!-- Insecure -->
    <Insecure Name="insecure" />
 
    <!-- SslCredentials -->
    <Ssl Name="ssl" RootCertificates=".\cert\exampleCA.crt">
      <Certificates>
        <Certificate CertificateChain=".\cert\exampleServer.crt" PrivateKey=".\cert\exampleServer.key" />
      </Certificates>
    </Ssl>

  </Credentials>

  <!-- Channels -->
  <Channels>
    <Channel Name="channel1" Host="127.0.0.1" Port="50051" CredentialsName="insecure" />
  </Channels>

  <!-- Services -->
  <Services>
    
    <Service Name="example1">

    <Interceptors>
        
        <!-- Refer to shared interceptor -->
        <Reference Refer="interceptor1" />
        <Reference Refer="interceptor2" />
        <Reference Refer="interceptor3" />

        <!-- Example of specifying a method to generate an interceptor -->
        <Custom Order="4" TypeName="Examples.GrpcConfiguration.Servers.Program, Examples.GrpcConfiguration.Servers" MethodName="CreateExampleInterceptor">
          <MethodArgs>
            <String Name="name" Value="serverIntercept4" />
          </MethodArgs>
        </Custom>

      </Interceptors>

      <ExtraInterceptors>

        <!-- Example of specifying a configuration corresponding to an interceptor -->
        <Extra xsi:type="ExampleInterceptorConfig" Order="5" InterceptorName="serverIntercept5" />

      </ExtraInterceptors>

    </Service>
  
  </Services>

</RpcConfigurationConfig>
```

#### Interceptor 

##### Interceptors

Specify the known configuration types. In this case you can only specify "Custom" (`CustomInterceptorConfig`).

* Method for generating interceptor.
  * This method must satisfy the following conditions.
    1. It is a static method.
    2. It is Func&lt;CustomInterceptorConfig, Interceptor&gt;.
* Type of interceptor.
  * The interceptor type must have a public constructor that takes no arguments.

##### ExtraInterceptors   

Specify arbitrary configuration types.

* Specify the config corresponding to the interceptor.
  * The configuration type must inherit `RpcInterceptorConfigBase`.
  * XmlSerializer must be able to recognize the configuration type.

#### Credential

##### Credentials

Specify the known configuration types. In this case you can specify "Insecure" (`CustomInterceptorConfig`) or "Ssl" ( `SslCredentialsConfig` ).


###### Insecure 

There is no setting value.

###### Ssl

Specify the values of RootCertificates, CertificateChain, PrivateKey attribute.
In the default behavior, the values set for these attributes are considered filenames for crt and key files, and the contents of that file are read.
To change this behavior, override RpcConfigurationContext.GetRootCertificates, RpcConfigurationContext.GetCertificateChain, RpcConfigurationContext.GetPrivateKey method or set the methods to get credentials to RpcConfigurationContext.RootCertificatesGetter, RpcConfigurationContext.CertificateChainGetter, RpcConfigurationContext.PrivateKeyGetter property.

#### ExtraCredentials

Specify arbitrary configuration types.

* Specify the config corresponding to the credentials.
  * The configuration type must inherit `RpcCredentialsConfigBase`.
  * XmlSerializer must be able to recognize the configuration type.

#### Channel

##### Channels

Specify the values represented by `RpcChannelConfig` class.
Specify the value of Name attribute of the credentials to use in CredentialsName attribute.

#### ServerServiceDefinition

##### Services

Specify the values represented by `RpcServiceConfig` class.

To apply an interceptor to a service, specify the interceptor to apply.
To use the shared interceptor, specify "Refer" (`InterceptorReference`).

### Service side implementation

```c#
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
```

### Client side configuration file

Serialize an instance of type `RpcConfigurationConfig` using XmlSerializer.

```xml:GrpcClient.config
<?xml version="1.0" encoding="utf-8"?>
<RpcConfigurationConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">

  <!-- Interceptors shared by multiple services -->
  <Interceptors>

    <!-- Example of specifying a method to generate an interceptor -->
    <Custom Name="interceptor1" TypeName="Examples.GrpcConfiguration.Clients.Program, Examples.GrpcConfiguration.Clients" MethodName="CreateExampleInterceptor">
      <MethodArgs>
        <String Name="name" Value="clientIntercept1" />
      </MethodArgs>
    </Custom>

    <!-- Example of specifying an interceptor type -->
    <Custom Name="interceptor2" TypeName="Examples.GrpcConfiguration.Models.ExampleInterceptor, Examples.GrpcConfiguration.Models" />

  </Interceptors>

  <ExtraInterceptors>

    <!-- Example of specifying a configuration corresponding to an interceptor -->
    <Extra xsi:type="ExampleInterceptorConfig" Name="interceptor3" InterceptorName="clientIntercept3" />

  </ExtraInterceptors>

  <!-- Credentials -->
  <Credentials>

    <!-- Insecure -->
    <Insecure Name="insecure" />
 
    <!-- SslCredentials -->
    <Ssl Name="ssl" RootCertificates=".\cert\exampleCA.crt">
      <Certificates>
        <Certificate CertificateChain=".\cert\exampleServer.crt" PrivateKey=".\cert\exampleServer.key" />
      </Certificates>
    </Ssl>

  </Credentials>

  <!-- Channels -->
  <Channels>
    <Channel Name="channel1" Host="127.0.0.1" Port="50051" CredentialsName="insecure" />
  </Channels>

  <!-- CallInvoker -->
  <CallInvokers>

    <Default Name="invoker1" ChannelName="channel1">
      
      <Interceptors>

        <!-- Refer to shared interceptor -->
        <Reference Order="1" Refer="interceptor1" />
        <Reference Order="2" Refer="interceptor2" />
        <Reference Order="3" Refer="interceptor3" />

        <!-- Example of specifying a method to generate an interceptor -->
        <Custom Order="4" TypeName="Examples.GrpcConfiguration.Clients.Program, Examples.GrpcConfiguration.Clients" MethodName="CreateExampleInterceptor">
          <MethodArgs>
            <String Name="name" Value="clientIntercept4" />
          </MethodArgs>
        </Custom>

      </Interceptors>

      <ExtraInterceptors>

        <!-- Example of specifying a configuration corresponding to an interceptor -->
        <Extra xsi:type="ExampleInterceptorConfig" Order="5" InterceptorName="clientIntercept5" />

      </ExtraInterceptors>

    </Default>
    
  </CallInvokers>

</RpcConfigurationConfig>
```

#### Interceptor 

Refer to the description of service side configuration file.

#### Credentials

Refer to the description of service side configuration file.

#### Channel

Refer to the description of service side configuration file.

#### CallInvoker

##### CallInvokers

Specify the configuration type corresponding to CallInvoker.
In this case you can specify "Default" (`DefaultCallInvokerConfig`) or "Custom" ( `CustomCallInvokerConfig` ).

To apply an interceptor to a callInvoler, specify the interceptor to apply.
To use the shared interceptor, specify "Refer" (`InterceptorReference`).

###### Default

There is no setting value.

###### Custom 

* Method for generating callInvoker.
  * This method must satisfy the following conditions.
    1. It is a static method.
    2. It is Func&lt;Channel, CallInvoker&gt;.
* Type of callInvoker.
  * The type of CallInvoker must have a public constructor that takes a Channel.

### Client side implementation

```c#
namespace Examples.GrpcConfiguration.Clients
{
    static class Program
    {
        /// <summary>
        ///
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

    /// <summary>
    /// 
    /// </summary>
    internal partial class Form1 : Form
    {

        #region ctor

        /// <summary>
        /// 
        /// </summary>
        internal Form1() : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcContext"></param>
        internal Form1(RpcConfigurationContext rpcContext)
        {
            InitializeComponent();
            m_RpcContext = rpcContext;
        }

        #endregion

        #region gRPC

        private readonly RpcConfigurationContext m_RpcContext;

        #endregion

        private async void BtnCallRpc_Click(object sender, EventArgs e)
        {
            try
            {

                CallInvoker invoker = m_RpcContext.GetCallInvoker("invoker1");

                ExampleService.ExampleServiceClient client = new ExampleService.ExampleServiceClient(invoker);

                MemberCondition condition = new MemberCondition() { ID = "123" };

                Models.Member member = await client.GetMemberAsync(condition);

                MessageBox.Show(member.Name);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message);
            }

        }
    }
}
```

### Implementation of the interceptor used in the above sample

#### ExampleInterceptor

It is only outputting messages to the debug console.

```c#:ExampleInterceptor.cs
namespace Examples.GrpcConfiguration.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class ExampleInterceptor : Interceptor
    {

        /// <summary>
        /// 
        /// </summary>
        public ExampleInterceptor() : this("")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ExampleInterceptor(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("[CLIENT][ExampleInterceptor:{0}] method={1} request={2}", Name, context.Method.FullName, request));
            return base.AsyncUnaryCall(request, context, continuation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("[SERVICE][ExampleInterceptor:{0}] method={1} request={2}", Name, context.Method, request));
            return base.UnaryServerHandler(request, context, continuation);
        }

    }

}
```

#### ExampleInterceptorConfig 

This is a configuration for generating ExampleInterceptor.

```c#:ExampleInterceptorConfig.cs
namespace Examples.GrpcConfiguration.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class ExampleInterceptorConfig : RpcInterceptorConfigBase
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        public string InterceptorName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Interceptor CreateInterceptor(RpcConfigurationContext context)
        {
            return new ExampleInterceptor(InterceptorName);
        }
    }

}
```

#### Sample execution result

ExampleInterceptor outputs debug messages.

```console
[CLIENT][ExampleInterceptor:clientIntercept1] method=/ExampleService/GetMember request={ "ID": "123" }
[CLIENT][ExampleInterceptor:] method=/ExampleService/GetMember request={ "ID": "123" }
[CLIENT][ExampleInterceptor:clientIntercept3] method=/ExampleService/GetMember request={ "ID": "123" }
[CLIENT][ExampleInterceptor:clientIntercept4] method=/ExampleService/GetMember request={ "ID": "123" }
[CLIENT][ExampleInterceptor:clientIntercept5] method=/ExampleService/GetMember request={ "ID": "123" }
[SERVICE][ExampleInterceptor:clientIntercept1] method=/ExampleService/GetMember request={ "ID": "123" }
[SERVICE][ExampleInterceptor:] method=/ExampleService/GetMember request={ "ID": "123" }
[SERVICE][ExampleInterceptor:serverIntercept3] method=/ExampleService/GetMember request={ "ID": "123" }
[SERVICE][ExampleInterceptor:serverIntercept4] method=/ExampleService/GetMember request={ "ID": "123" }
[SERVICE][ExampleInterceptor:serverIntercept5] method=/ExampleService/GetMember request={ "ID": "123" }
```
