using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="ChannelCredentials"/>.
    /// </summary>
    [XmlInclude(typeof(Credentials.InsecureCredentialsConfig))]
    [XmlInclude(typeof(Credentials.SslCredentialsConfig))]
    public abstract class RpcCredentialsConfigBase
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        #region activation

        /// <summary>
        /// Create a <see cref="ChannelCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract ChannelCredentials CreateChannelCredentials(RpcConfigurationContext context);

        /// <summary>
        /// Create a <see cref="ServerCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract ServerCredentials CreateServerCredentials(RpcConfigurationContext context);

        #endregion

    }

}
