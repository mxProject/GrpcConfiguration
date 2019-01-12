using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of gRPC client and service.
    /// </summary>
    public class RpcConfigurationConfig
    {
        
        #region interceptor

        /// <summary>
        /// Gets or sets the interceptor configs.
        /// </summary>
        [XmlArrayItem("Custom", typeof(Interceptors.CustomInterceptorConfig))]
        public RpcInterceptorConfigBase[] Interceptors { get; set; }

        /// <summary>
        /// Gets or sets any types of interceptor settings inherited from <see cref="RpcInterceptorConfigBase"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You need to enable XmlSerializer to recognize information of the types specified in this property.
        /// </para>
        /// </remarks>
        [XmlArrayItem("Extra")]
        public RpcInterceptorConfigBase[] ExtraInterceptors { get; set; }

        #endregion

        #region credential

        /// <summary>
        /// Gets or sets the credential configs.
        /// </summary>
        [XmlArrayItem("Insecure", typeof(Credentials.InsecureCredentialsConfig))]
        [XmlArrayItem("Ssl", typeof(Credentials.SslCredentialsConfig))]
        public RpcCredentialsConfigBase[] Credentials { get; set; }

        /// <summary>
        /// Gets or sets any types of credential settings inherited from <see cref="RpcCredentialsConfigBase"/>.
        /// </summary>
        /// <remarks>
        /// You need to enable XmlSerializer to recognize information of the types specified in this property.
        /// </remarks>
        [XmlArrayItem("Extra")]
        public RpcCredentialsConfigBase[] ExtraCredentials { get; set; }

        #endregion

        #region channel

        /// <summary>
        /// Gets or sets the channel configs.
        /// </summary>
        [XmlArrayItem("Channel")]
        public RpcChannelConfig[] Channels { get; set; }

        #endregion

        #region callInvoker

        /// <summary>
        /// Gets or sets the callInvoker configs.
        /// </summary>
        [XmlArrayItem("Default", typeof(CallInvokers.DefaultCallInvokerConfig))]
        [XmlArrayItem("Custom", typeof(CallInvokers.CustomCallInvokerConfig))]
        public RpcCallInvokerConfigBase[] CallInvokers { get; set; }

        /// <summary>
        /// Gets or sets any types of callInvoker settings inherited from <see cref="RpcCallInvokerConfigBase"/>.
        /// </summary>
        /// <remarks>
        /// You need to enable XmlSerializer to recognize information of the types specified in this property.
        /// </remarks>
        [XmlArrayItem("Extra")]
        public RpcCallInvokerConfigBase[] ExtraCallInvokers { get; set; }

        #endregion

        #region service

        /// <summary>
        /// Gets or sets the service configs.
        /// </summary>
        [XmlArrayItem("Service")]
        public RpcServiceConfig[] Services { get; set; }

        #endregion

    }

}
