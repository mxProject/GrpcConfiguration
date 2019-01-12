using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.CallInvokers
{

    /// <summary>
    /// Config of custom callInvoker.
    /// </summary>
    public sealed class CustomCallInvokerConfig : RpcCallInvokerConfigBase
    {

        /// <summary>
        /// Gets or sets the type name where the method generating the callInvoker is declared.
        /// </summary>
        [XmlAttribute]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the method that generates the callInvoker.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must satisfy the following conditions.
        /// 1) It is a static method.
        /// 2) It is <see cref="Func{Channel, CallInvoker}"/>.
        /// </para>
        /// <para>
        /// If the value of this property is not set, the callInvoker is generated using the constructor of the type specified by <see cref="TypeName"/> property.
        /// </para>
        /// </remarks>
        [XmlAttribute]
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets arguments of the method that generates the callInvoker.
        /// </summary>
        [XmlArrayItem("Boolean", typeof(KeyValues.BooleanValueConfig))]
        [XmlArrayItem("Byte", typeof(KeyValues.ByteValueConfig))]
        [XmlArrayItem("Int16", typeof(KeyValues.Int16ValueConfig))]
        [XmlArrayItem("Int32", typeof(KeyValues.Int32ValueConfig))]
        [XmlArrayItem("Int64", typeof(KeyValues.Int64ValueConfig))]
        [XmlArrayItem("Single", typeof(KeyValues.SingleValueConfig))]
        [XmlArrayItem("Double", typeof(KeyValues.DoubleValueConfig))]
        [XmlArrayItem("Decimal", typeof(KeyValues.DecimalValueConfig))]
        [XmlArrayItem("Char", typeof(KeyValues.CharValueConfig))]
        [XmlArrayItem("String", typeof(KeyValues.StringValueConfig))]
        [XmlArrayItem("DateTime", typeof(KeyValues.DateTimeValueConfig))]
        public KeyValueConfigBase[] MethodArgs { get; set; }

        #region activation

        /// <summary>
        /// Create a callInvoker.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="RpcConfigurationException">
        /// Could not get the type information.
        /// </exception>
        /// <exception cref="RpcConfigurationException">
        /// Could not get the method information.
        /// </exception>
        /// <exception cref="RpcConfigurationException">
        /// The method threw an exception.
        /// </exception>
        /// <returns></returns>
        public override CallInvoker CreateRootCallInvoker(Channel channel, RpcConfigurationContext context)
        {
            Type type = Type.GetType(TypeName);

            if (type == null)
            {
                throw new RpcConfigurationException(string.Format("Could not get the type information. TypeName is '{0}'.", TypeName));
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                ConstructorInfo ctor = type.GetTypeInfo().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Channel) }, null);

                if (type == null)
                {
                    throw new RpcConfigurationException(string.Format("Could not get the constructor information. TypeName is '{0}'.", TypeName));
                }

                try
                {
                    return (CallInvoker)ctor.Invoke(new object[] { channel });
                }
                catch (Exception ex)
                {
                    throw new RpcConfigurationException(string.Format("The constructor threw an exception. TypeName is '{0}'. {1}", TypeName, ex.Message), ex);
                }
            }
            else
            {
                MethodInfo method = type.GetTypeInfo().GetMethod(MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Channel) }, null);

                if (method == null)
                {
                    throw new RpcConfigurationException(string.Format("Could not get the method information. MethodName is '{0}.{1}'.", type.Name, MethodName));
                }

                try
                {
                    return (CallInvoker)method.Invoke(null, new object[] { channel });
                }
                catch (Exception ex)
                {
                    throw new RpcConfigurationException(string.Format("The method threw an exception. MethodName is '{0}.{1}'. {2}", type.Name, MethodName, ex.Message), ex);
                }
            }

        }

        /// <summary>
        /// Gets the argument value of the method that generates the callInvoker.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The argument value.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetMethodArgs(string name, out object value)
        {
            if (MethodArgs != null)
            {
                string key = RpcConfigurationUtility.NormalizeName(name);

                foreach (KeyValueConfigBase keyValue in MethodArgs)
                {
                    if (name == RpcConfigurationUtility.NormalizeName(keyValue.Name))
                    {
                        value = keyValue.GetValue();
                        return true;
                    }
                }
            }
            value = null;
            return false;
        }

        #endregion

    }

}
