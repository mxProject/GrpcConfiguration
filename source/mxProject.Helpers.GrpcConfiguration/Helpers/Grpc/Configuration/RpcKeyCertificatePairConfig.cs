using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config or <see cref="KeyCertificatePair"/>
    /// </summary>
    public sealed class RpcKeyCertificatePairConfig
    {

        /// <summary>
        /// Gets or sets PEM encoded certificate chain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property is passed to <see cref="RpcConfigurationContext.GetCertificateChain"/> method and is used to get the credentials information. 
        /// The default <see cref="RpcConfigurationContext.GetCertificateChain"/> method considers the value of this property as a file path.
        /// </para>
        /// </remarks>
        [XmlAttribute]
        public string CertificateChain { get; set; }

        /// <summary>
        /// Gets or sets PEM encoded private key.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property is passed to <see cref="RpcConfigurationContext.GetPrivateKey"/> method and is used to get the credentials information. 
        /// The default <see cref="RpcConfigurationContext.GetPrivateKey"/> method considers the value of this property as a file path.
        /// </para>
        /// </remarks>
        [XmlAttribute]
        public string PrivateKey { get; set; }

        #region activation

        /// <summary>
        /// Create a KeyCertificatePair.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public KeyCertificatePair CreateKeyCertificatePair(RpcConfigurationContext context)
        {
            return new KeyCertificatePair(context.GetCertificateChain(CertificateChain), context.GetPrivateKey(PrivateKey));
        }

        #endregion

    }

}
