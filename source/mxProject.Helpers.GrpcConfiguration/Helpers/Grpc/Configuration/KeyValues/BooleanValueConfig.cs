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
    public class BooleanValueConfig : KeyValueConfigBase
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute]
        public bool Value
        {
            get { return (bool)InternalValue; }
            set { InternalValue = value; }
        }

    }

}
