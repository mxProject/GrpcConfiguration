using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration.Interceptors
{

    /// <summary>
    /// Config of custom Interceptor
    /// </summary>
    public sealed class InterceptorReference : RpcInterceptorConfigBase
    {

        /// <summary>
        /// Gets or sets the name of the interceptor to be referenced.
        /// </summary>
        [XmlAttribute("Refer")]
        public string ReferencingName { get; set; }

        #region activation

        /// <summary>
        /// Create a Interceptor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        /// <returns></returns>
        public override Interceptor CreateInterceptor(RpcConfigurationContext context)
        {
            if (context.TryGetInterceptor(ReferencingName, out Interceptor interceptor))
            {
                return interceptor;
            }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", ReferencingName));
        }

        #endregion

    }

}
