using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Config of a key and value pair.
    /// </summary>
    [XmlInclude(typeof(KeyValues.BooleanValueConfig))]
    [XmlInclude(typeof(KeyValues.ByteValueConfig))]
    [XmlInclude(typeof(KeyValues.Int16ValueConfig))]
    [XmlInclude(typeof(KeyValues.Int32ValueConfig))]
    [XmlInclude(typeof(KeyValues.Int64ValueConfig))]
    [XmlInclude(typeof(KeyValues.SingleValueConfig))]
    [XmlInclude(typeof(KeyValues.DoubleValueConfig))]
    [XmlInclude(typeof(KeyValues.DecimalValueConfig))]
    [XmlInclude(typeof(KeyValues.CharValueConfig))]
    [XmlInclude(typeof(KeyValues.StringValueConfig))]
    [XmlInclude(typeof(KeyValues.DateTimeValueConfig))]
    public abstract class KeyValueConfigBase
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        protected object InternalValue { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return InternalValue;
        }

    }

}
