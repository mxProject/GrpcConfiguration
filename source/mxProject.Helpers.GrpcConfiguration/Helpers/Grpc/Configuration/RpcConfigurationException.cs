using System;
using System.Collections.Generic;
using System.Text;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// Represents errors that occur during gRPC configuration.
    /// </summary>
    public class RpcConfigurationException : Exception
    {

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RpcConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RpcConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

}
