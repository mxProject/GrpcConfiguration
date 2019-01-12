using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of <see cref="ChannelOption"/>.
    /// </summary>
    [XmlInclude(typeof(ChannelOptions.Int32ChannelOptionConfig))]
    [XmlInclude(typeof(ChannelOptions.StringChannelOptionConfig))]
    public abstract class RpcChannelOptionValueConfigBase
    {

        /// <summary>
        /// Gets or sets the name in the configuration.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        #region activation

        /// <summary>
        /// Create a <see cref="ChannelOption"/>.
        /// </summary>
        /// <returns></returns>
        public abstract ChannelOption CreateChannelOption();

        #endregion

    }

}
