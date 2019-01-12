using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Examples.GrpcConfiguration.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class ExampleInterceptor : Interceptor
    {

        /// <summary>
        /// 
        /// </summary>
        public ExampleInterceptor() : this("")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ExampleInterceptor(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("[CLIENT][ExampleInterceptor:{0}] method={1} request={2}", Name, context.Method.FullName, request));
            return base.AsyncUnaryCall(request, context, continuation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("[SERVICE][ExampleInterceptor:{0}] method={1} request={2}", Name, context.Method, request));
            return base.UnaryServerHandler(request, context, continuation);
        }

    }

}
