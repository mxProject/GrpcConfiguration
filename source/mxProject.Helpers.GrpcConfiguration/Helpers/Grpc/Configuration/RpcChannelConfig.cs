using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="Channel"/>.
    /// </summary>
    public sealed class RpcChannelConfig
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        [XmlAttribute]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        [XmlAttribute]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the credentials name to use.
        /// </summary>
        [XmlAttribute]
        public string CredentialsName { get; set; }

        /// <summary>
        /// Gets or sets the channelOption configs.
        /// </summary>
        [XmlArrayItem("Int32", typeof(ChannelOptions.Int32ChannelOptionConfig))]
        [XmlArrayItem("String", typeof(ChannelOptions.StringChannelOptionConfig))]
        public RpcChannelOptionValueConfigBase[] Options { get; set; }

        #region activation

        /// <summary>
        /// Create a <see cref="Channel"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Channel CreateChannel(RpcConfigurationContext context)
        {
            if (Options == null || Options.Length == 0)
            {
                return new Channel(Host, Port, CreateChannelCredentials(context));
            }
            else
            {
                return new Channel(Host, Port, CreateChannelCredentials(context), CreateChannelOptions());
            }
        }

        /// <summary>
        /// Create a <see cref="ServerPort"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public ServerPort CreateServerPort(RpcConfigurationContext context)
        {
            return new ServerPort(Host, Port, CreateServerCredentials(context));
        }

        /// <summary>
        /// Create a <see cref="ChannelCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        /// <returns></returns>
        private ChannelCredentials CreateChannelCredentials(RpcConfigurationContext context)
        {
            if (string.IsNullOrEmpty(CredentialsName))
            {
                return ChannelCredentials.Insecure;
            }

            if (context.TryGetChannelCredentials(CredentialsName, out ChannelCredentials credentials))
            {
                return credentials;
            }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", CredentialsName));
        }

        /// <summary>
        /// Create a <see cref="ServerCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private ServerCredentials CreateServerCredentials(RpcConfigurationContext context)
        {
            if (string.IsNullOrEmpty(CredentialsName))
            {
                return ServerCredentials.Insecure;
            }

            if (context.TryGetServerCredentials(CredentialsName, out ServerCredentials credentials))
            {
                return credentials;
            }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", CredentialsName));
        }

        /// <summary>
        /// Create <see cref="ChannelOption"/>.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ChannelOption> CreateChannelOptions()
        {
            if (Options == null || Options.Length == 0) { yield break; }

            foreach (RpcChannelOptionValueConfigBase config in Options)
            {
                yield return config.CreateChannelOption();
            }
        }

        #endregion

    }

}
