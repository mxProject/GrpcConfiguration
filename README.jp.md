

# mxProject.Helpers.GrpcConfiguration

XML ファイルで gRPC の構成を行うためのライブラリです。

## 機能

* XMLファイルの設定から次のオブジェクトを生成することができます。
  * CallInvoker
  * Channel
  * ChannelCredentials
  * ServerPort
  * ServerCredentials
  * Interceptor


## 依存バージョン

* .NET Framework >= 4.6
* .NET Standard >= 2.0
* Grpc.Core >= 1.16.0

## ライセンス

[MIT ライセンス](http://opensource.org/licenses/mit-license.php)

## 使用例

### サービス側の設定ファイル

`RpcConfigurationConfig` クラスのインスタンスを XmlSerializer でシリアライズした XML ファイルです。

```xml:サンプル
<?xml version="1.0" encoding="utf-8"?>
<RpcConfigurationConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">

  <!-- サービス間で共有するインターセプター -->
  <Interceptors>

    <!-- インターセプターを生成するメソッドを指定する例 -->
    <Custom Name="interceptor1" TypeName="Examples.GrpcConfiguration.Servers.Program, Examples.GrpcConfiguration.Servers" MethodName="CreateExampleInterceptor">
      <MethodArgs>
        <String Name="name" Value="clientIntercept1" />
      </MethodArgs>
    </Custom>

    <!-- インターセプターの型を指定する例 -->
    <Custom Name="interceptor2" TypeName="Examples.GrpcConfiguration.Models.ExampleInterceptor, Examples.GrpcConfiguration.Models" />

  </Interceptors>

  <ExtraInterceptors>

    <!-- インターセプターに対応するコンフィグを指定する例 -->
    <Extra xsi:type="ExampleInterceptorConfig" Name="interceptor3" InterceptorName="serverIntercept3" />

  </ExtraInterceptors>

  <!-- 資格証明 -->
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

  <!-- チャネル -->
  <Channels>
    <Channel Name="channel1" Host="127.0.0.1" Port="50051" CredentialsName="insecure" />
  </Channels>

  <!-- サービス -->
  <Services>
    
    <Service Name="example1">

    <Interceptors>
        
        <!-- 共有インターセプターを参照する例 -->
        <Reference Refer="interceptor1" />
        <Reference Refer="interceptor2" />
        <Reference Refer="interceptor3" />

        <!-- インターセプターを生成するメソッドを指定する例 -->
        <Custom Order="4" TypeName="Examples.GrpcConfiguration.Servers.Program, Examples.GrpcConfiguration.Servers" MethodName="CreateExampleInterceptor">
          <MethodArgs>
            <String Name="name" Value="serverIntercept4" />
          </MethodArgs>
        </Custom>

      </Interceptors>

      <ExtraInterceptors>

        <!-- インターセプターに対応するコンフィグを指定する例 -->
        <Extra xsi:type="ExampleInterceptorConfig" Order="5" InterceptorName="serverIntercept5" />

      </ExtraInterceptors>

    </Service>
  
  </Services>

</RpcConfigurationConfig>
```

#### インターセプター 

##### Interceptors

既知のコンフィグ型を指定します。ここでは Custom ( `CustomInterceptorConfig` ) のみを指定できます。

* インターセプターを生成するメソッドを指定する。
  * メソッドは static な Func&lt;CustomInterceptorConfig, Interceptor&gt; である必要があります。
  * public / nonpublic は問いません。
* インターセプターの型を指定する。
  * インターセプターの型は引数をとらない public コンストラクタを持つ必要があります。

##### ExtraInterceptors   

任意のコンフィグ型を指定します。

* インターセプターに対応するコンフィグを指定する。
  * コンフィグの型は `RpcInterceptorConfigBase` を継承している必要があります。
  * XmlSerializer がそのコンフィグの型を認識できる必要があります。

#### 資格証明

##### Credentials

既知のコンフィグ型を指定します。ここでは Insecure ( `InsecureCredentialsConfig` ), Ssl ( `SslCredentialsConfig` ) を指定できます。

###### Insecure 

設定値はありません。

###### Ssl

RootCertificates, CertificateChain, PrivateKey 属性に資格証明情報を設定します。既定の動作ではこれらの属性に設定された値は crt や key ファイルのファイル名と見なし、そのファイルの内容を読み込みます。この動作を変更するには、`RpcConfigurationContext` クラスの GetRootCertificates, GetCertificateChain, GetPrivateKey メソッドをオーバーライドするか、RootCertificatesGetter,  CertificateChainGetter, PrivateKeyGetter プロパティに資格証明を取得するためのメソッドを設定します。

#### ExtraCredentials

任意のコンフィグ型を指定します。

* 資格証明に対応するコンフィグを指定する。
  * コンフィグの型は `RpcCredentialsConfigBase` を継承している必要があります。
  * XmlSerializer がそのコンフィグの型を認識できる必要があります。

#### チャネル

##### Channels

`RpcChannelConfig` クラスで表される設定値を指定します。
CredentialsName 属性には使用する資格証明の Name 属性の値を指定します。

#### サービス

##### Services

`RpcServiceConfig` クラスで表される設定値を指定します。

サービスに対してインターセプターを適用する場合、適用するインターセプターを指定します。
Intercepters, ExtraInterceptors の設定方法は共有インターセプターの設定方法と同じです。
共有インターセプターを使用する場合、Interceptors に Refer ( `InterceptorReference` ) を指定します。Refer 属性に共有インターセプターの Name 属性の値を指定します。

### サービス側の実装

```c#:サンプル
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
            // 設定ファイルを読み込みます。
            RpcConfigurationConfig config = LoadConfig(@".\GrpcServer.config");

            // コンテキストを生成します。
            // 第二引数は、コンテキストが dispose された時に生成した gRPC オブジェクトを破棄（チャネルをシャットダウン）するかどうかを表します。
            using (RpcConfigurationContext context = new RpcConfigurationContext(config, true))
            {

                Server server = new Server();

                // 設定上のサービス名を指定してサービスに対してインターセプターを適用します。
                server.Services.Add(context.Intercept(ExampleService.BindService(new ExampleServiceImpl()), "example1"));

                // 設定上のチャネル名を指定してサーバーポートを生成します。
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
        /// 指定された設定ファイルを読み込みます。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static RpcConfigurationConfig LoadConfig(string filePath)
        {
            // ExtraInterceptors などに指定した型をシリアライザに渡します。
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
        /// インターセプターを生成します。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Interceptor CreateExampleInterceptor(CustomInterceptorConfig config)
        {
            // MethodArgs で指定した引数を取得できます。
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

### クライアント側の設定ファイル

こちらも `RpcConfigurationConfig` クラスのインスタンスを XmlSerializer でシリアライズした XML ファイルです。

```xml:サンプル
<?xml version="1.0" encoding="utf-8"?>
<RpcConfigurationConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">

  <!-- サービス間で共有するインターセプター -->
  <Interceptors>

    <!-- インターセプターを生成するメソッドを指定する例 -->
    <Custom Name="interceptor1" TypeName="Examples.GrpcConfiguration.Clients.Program, Examples.GrpcConfiguration.Clients" MethodName="CreateExampleInterceptor">
      <MethodArgs>
        <String Name="name" Value="clientIntercept1" />
      </MethodArgs>
    </Custom>

    <!-- インターセプターの型を指定する例 -->
    <Custom Name="interceptor2" TypeName="Examples.GrpcConfiguration.Models.ExampleInterceptor, Examples.GrpcConfiguration.Models" />

  </Interceptors>

  <ExtraInterceptors>

    <!-- インターセプターに対応するコンフィグを指定する例 -->
    <Extra xsi:type="ExampleInterceptorConfig" Name="interceptor3" InterceptorName="clientIntercept3" />

  </ExtraInterceptors>

  <!-- 資格証明 -->
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

  <!-- チャネル -->
  <Channels>
    <Channel Name="channel1" Host="127.0.0.1" Port="50051" CredentialsName="insecure" />
  </Channels>

  <!-- CallInvoker -->
  <CallInvokers>

    <Default Name="invoker1" ChannelName="channel1">
      
      <Interceptors>

        <!-- 共有インターセプターを参照する例 -->
        <Reference Order="1" Refer="interceptor1" />
        <Reference Order="2" Refer="interceptor2" />
        <Reference Order="3" Refer="interceptor3" />

        <!-- インターセプターを生成するメソッドを指定する例 -->
        <Custom Order="4" TypeName="Examples.GrpcConfiguration.Clients.Program, Examples.GrpcConfiguration.Clients" MethodName="CreateExampleInterceptor">
          <MethodArgs>
            <String Name="name" Value="clientIntercept4" />
          </MethodArgs>
        </Custom>

      </Interceptors>

      <ExtraInterceptors>

        <!-- インターセプターに対応するコンフィグを指定する例 -->
        <Extra xsi:type="ExampleInterceptorConfig" Order="5" InterceptorName="clientIntercept5" />

      </ExtraInterceptors>

    </Default>
    
  </CallInvokers>

</RpcConfigurationConfig>
```

#### インターセプター 

サービス側の設定ファイルの説明を参照してください。

#### 資格証明

サービス側の設定ファイルの説明を参照してください。

#### チャネル

サービス側の設定ファイルの説明を参照してください。

#### CallInvoker

##### CallInvokers

CallInvoker に対応するコンフィグ型を指定します。ここでは Default ( `DefaultCallInvokerConfig` ), Custom ( `CustomCallInvokerConfig` ) を指定できます。

###### Default

設定する値はありません。

###### Custom 

* CallInvoker を生成するメソッドを指定する。
  * メソッドは static な Func&lt;Channel, CallInvoker&gt; である必要があります。
  * public / nonpublic は問いません。
* CallInvoker の型を指定する。
  * CallInvoker の型は Channel を受け取る public コンストラクタを持つ必要があります。

CallInvoker に対してインターセプターを適用する場合、適用するインターセプターを指定します。
Intercepters, ExtraInterceptors の設定方法は共有インターセプターの設定方法と同じです。
共有インターセプターを使用する場合、Interceptors に Refer ( `InterceptorReference` ) を指定します。Refer 属性に共有インターセプターの Name 属性の値を指定します。

### クライアント側の実装

```c#:サンプル
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

            // 設定ファイルを読み込みます。
            RpcConfigurationConfig config = LoadConfig(@".\GrpcClient.config");

            // コンテキストを生成します。
            // 第二引数は、コンテキストが dispose された時に生成した gRPC オブジェクトを破棄（チャネルをシャットダウン）するかどうかを表します。
            using (RpcConfigurationContext context = new RpcConfigurationContext(config, true))
            {
                Application.Run(new Form1(context));
            }

        }

        /// <summary>
        /// 指定された設定ファイルを読み込みます。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static RpcConfigurationConfig LoadConfig(string filePath)
        {
            // ExtraInterceptors などに指定した型をシリアライザに渡します。
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
        /// インターセプターを生成します。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Interceptor CreateExampleInterceptor(CustomInterceptorConfig config)
        {
            // MethodArgs で指定した引数を取得できます。
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

namespace Examples.GrpcConfiguration.Clients
{

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

                // 設定上の名称を指定して CallInvoker を生成します。
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

### 上記のサンプルで使用しているインターセプターの実装

#### ExampleInterceptor

デバッグコンソールにメッセージを出力しているだけです。

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

ExampleInterceptor を生成するコンフィグです。

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

#### サンプルの実行結果

ExampleInterceptor によってデバッグメッセージが出力されます。

```console:デバッグコンソール
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
