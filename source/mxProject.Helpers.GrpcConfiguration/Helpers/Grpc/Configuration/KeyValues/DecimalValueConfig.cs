using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace mxProject.Helpers.Grpc.Configuration.KeyValues
{

    /// <summary>
    /// Config of a key and value pair.
    /// </summary>
    public class DecimalValueConfig : KeyValueConfigBase
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute]
        public decimal Value
        {
            get { return (decimal)InternalValue; }
            set { InternalValue = value; }
        }

    }

}
