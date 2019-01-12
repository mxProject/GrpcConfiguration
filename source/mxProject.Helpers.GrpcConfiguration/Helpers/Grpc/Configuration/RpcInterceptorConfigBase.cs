using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="Interceptor"/>.
    /// </summary>
    [XmlInclude(typeof(Interceptors.CustomInterceptorConfig))]
    [XmlInclude(typeof(Interceptors.InterceptorReference))]
    public abstract class RpcInterceptorConfigBase
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of interception.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public int Order { get; set; }

        #region activation

        /// <summary>
        /// Create a <see cref="Interceptor"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract Interceptor CreateInterceptor(RpcConfigurationContext context);

        #endregion

        #region misc

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static int CompareByOrder(RpcInterceptorConfigBase a, RpcInterceptorConfigBase b)
        {
            if (a != null && b != null)
            {
                return a.Order.CompareTo(b.Order);
            }
            else if (a != null)
            {
                return 1;
            }
            else if (b != null)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        #endregion

    }

}
