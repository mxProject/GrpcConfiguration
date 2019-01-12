using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.ChannelOptions
{

    /// <summary>
    /// Config of <see cref="ChannelOption"/> with a string value.
    /// </summary>
    public sealed class StringChannelOptionConfig : RpcChannelOptionValueConfigBase
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute]
        public string Value { get; set; }

        #region activation

        /// <summary>
        /// Create a ChannelOption.
        /// </summary>
        /// <returns></returns>
        public override ChannelOption CreateChannelOption()
        {
            return new ChannelOption(Name, Value);
        }

        #endregion

    }

}
