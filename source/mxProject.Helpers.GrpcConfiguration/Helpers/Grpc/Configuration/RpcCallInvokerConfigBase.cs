using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="CallInvoker"/>.
    /// </summary>
    [XmlInclude(typeof(CallInvokers.DefaultCallInvokerConfig))]
    [XmlInclude(typeof(CallInvokers.CustomCallInvokerConfig))]
    public abstract class RpcCallInvokerConfigBase
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the channel name to use.
        /// </summary>
        [XmlAttribute]
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets the interceptor settings.
        /// </summary>
        [XmlArrayItem("Custom", typeof(Interceptors.CustomInterceptorConfig))]
        [XmlArrayItem("Reference", typeof(Interceptors.InterceptorReference))]
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

        #region activation

        /// <summary>
        /// Create a <see cref="CallInvoker"/>.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public CallInvoker CreateCallInvoker(Channel channel, RpcConfigurationContext context)
        {
            Interceptor[] interceptors = RpcConfigurationUtility.CreateInterceptors(context, Interceptors, ExtraInterceptors);

            if (interceptors.Length > 0)
            {
                return CreateRootCallInvoker(channel, context).Intercept(interceptors);
            }
            else
            {
                return CreateRootCallInvoker(channel, context);
            }
        }

        /// <summary>
        /// Create a <see cref="CallInvoker"/>.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract CallInvoker CreateRootCallInvoker(Channel channel, RpcConfigurationContext context);

        #endregion

    }

}
