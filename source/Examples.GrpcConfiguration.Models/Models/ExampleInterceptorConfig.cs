using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grpc.Core.Interceptors;
using mxProject.Helpers.Grpc.Configuration;

namespace Examples.GrpcConfiguration.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class ExampleInterceptorConfig : RpcInterceptorConfigBase
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        public string InterceptorName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Interceptor CreateInterceptor(RpcConfigurationContext context)
        {
            return new ExampleInterceptor(InterceptorName);
        }
    }

}
