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
    public class Int16ValueConfig : KeyValueConfigBase
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute]
        public short Value
        {
            get { return (short)InternalValue; }
            set { InternalValue = value; }
        }

    }

}
