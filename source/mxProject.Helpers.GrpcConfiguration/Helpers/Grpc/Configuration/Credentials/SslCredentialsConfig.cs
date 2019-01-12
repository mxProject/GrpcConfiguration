using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.Credentials
{

    /// <summary>
    /// Config of <see cref="SslCredentials"/>.
    /// </summary>
    public sealed class SslCredentialsConfig : RpcCredentialsConfigBase
    {

        /// <summary>
        /// Gets or sets PEM encoding of the server root certificates.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property is passed to <see cref="RpcConfigurationContext.GetRootCertificates"/> method and is used to get the credentials information. 
        /// The default <see cref="RpcConfigurationContext.GetRootCertificates"/> method considers the value of this property as a file path.
        /// </para>
        /// </remarks>
        [XmlAttribute]
        public string RootCertificates { get; set; }

        /// <summary>
        /// Gets or sets client side key and certificate pair. If null, client will not use key and certificate pair.
        /// </summary>
        [XmlArrayItem("Certificate")]
        public RpcKeyCertificatePairConfig[] Certificates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the mode of requesting certificate from client by the server.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(typeof(SslClientCertificateRequestType), "DontRequest")]
        public SslClientCertificateRequestType RequestType { get; set; } = SslClientCertificateRequestType.DontRequest;

        #region activation

        /// <summary>
        /// Create a <see cref="ChannelCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ChannelCredentials CreateChannelCredentials(RpcConfigurationContext context)
        {
            string root = context.GetRootCertificates(RootCertificates);

            if (Certificates == null || Certificates.Length == 0)
            {
                return new SslCredentials(root);
            }
            else
            {
                return new SslCredentials(root, Certificates[0].CreateKeyCertificatePair(context));
            }
        }

        /// <summary>
        /// Create a <see cref="ServerCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ServerCredentials CreateServerCredentials(RpcConfigurationContext context)
        {
            string root = context.GetRootCertificates(RootCertificates);

            if (string.IsNullOrEmpty(root))
            {
                return new SslServerCredentials(CreateKeyCertificatePairs(Certificates, context));
            }
            else
            {
                return new SslServerCredentials(CreateKeyCertificatePairs(Certificates, context), root, RequestType);
            }
        }

        /// <summary>
        /// Create <see cref="KeyCertificatePair"/>.
        /// </summary>
        /// <param name="configs"></param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private IEnumerable<KeyCertificatePair> CreateKeyCertificatePairs(IEnumerable<RpcKeyCertificatePairConfig> configs, RpcConfigurationContext context)
        {
            if (configs == null) { yield break; }

            foreach (RpcKeyCertificatePairConfig config in configs)
            {
                yield return config.CreateKeyCertificatePair(context);
            }
        }

        #endregion

    }

}
