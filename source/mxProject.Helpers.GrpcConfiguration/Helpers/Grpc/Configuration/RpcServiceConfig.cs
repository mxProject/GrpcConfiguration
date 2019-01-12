using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="ServerServiceDefinition"/>.
    /// </summary>
    public sealed class RpcServiceConfig
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

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
        /// Intercept the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public ServerServiceDefinition Intercept(ServerServiceDefinition service, RpcConfigurationContext context)
        {
            Interceptor[] interceptors = RpcConfigurationUtility.CreateInterceptors(context, Interceptors, ExtraInterceptors);

            if (interceptors.Length > 0)
            {
                return service.Intercept(interceptors);
            }
            else
            {
                return service;
            }
        }

        #endregion

    }

}
