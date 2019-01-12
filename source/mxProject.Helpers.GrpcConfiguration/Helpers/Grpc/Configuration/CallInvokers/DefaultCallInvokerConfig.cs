using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.CallInvokers
{

    /// <summary>
    /// Config of <see cref="DefaultCallInvoker"/>.
    /// </summary>
    public sealed class DefaultCallInvokerConfig : RpcCallInvokerConfigBase
    {

        #region activation

        /// <summary>
        /// Create a CallInvoker.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override CallInvoker CreateRootCallInvoker(Channel channel, RpcConfigurationContext context)
        {
            return new DefaultCallInvoker(channel);
        }

        #endregion

    }

}
