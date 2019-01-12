using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace mxProject.Helpers.Grpc.Configuration.Credentials
{

    /// <summary>
    /// Config of <see cref="ChannelCredentials.Insecure"/>.
    /// </summary>
    public sealed class InsecureCredentialsConfig : RpcCredentialsConfigBase
    {

        #region activation

        /// <summary>
        /// Create a <see cref="ChannelCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ChannelCredentials CreateChannelCredentials(RpcConfigurationContext context)
        {
            return ChannelCredentials.Insecure;
        }

        /// <summary>
        /// Create a <see cref="ServerCredentials"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ServerCredentials CreateServerCredentials(RpcConfigurationContext context)
        {
            return ServerCredentials.Insecure;
        }

        #endregion

    }

}
