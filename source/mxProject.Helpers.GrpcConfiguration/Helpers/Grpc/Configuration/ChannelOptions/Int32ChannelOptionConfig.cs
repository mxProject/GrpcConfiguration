using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.ChannelOptions
{

    /// <summary>
    /// Config of <see cref="ChannelOption"/> with a integer value.
    /// </summary>
    public sealed class Int32ChannelOptionConfig : RpcChannelOptionValueConfigBase
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute]
        public int Value { get; set; }

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
