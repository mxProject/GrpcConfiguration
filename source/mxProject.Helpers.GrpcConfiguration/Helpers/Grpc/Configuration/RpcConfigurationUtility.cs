using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// 
    /// </summary>
    internal static class RpcConfigurationUtility
    {

        /// <summary>
        /// Normalize the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string NormalizeName(string name)
        {
            return name.ToLower();
        }

        /// <summary>
        /// Enumerate all objects in the specified collections.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collections"></param>
        /// <returns></returns>
        internal static IEnumerable<T> EnumerateAll<T>(params IEnumerable<T>[] collections)
        {
            foreach (IEnumerable<T> collection in collections)
            {
                if (collection == null) { continue; }
                foreach (T obj in collection)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Create interceptors.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="interceptorConfigs">The interceptor configs.</param>
        /// <returns></returns>
        internal static Interceptor[] CreateInterceptors(RpcConfigurationContext context, params RpcInterceptorConfigBase[][] interceptorConfigs)
        {
            List<RpcInterceptorConfigBase> configs = new List<RpcInterceptorConfigBase>();

            foreach (RpcInterceptorConfigBase[] config in interceptorConfigs)
            {
                if (config != null) { configs.AddRange(config); }
            }

            configs.Sort(RpcInterceptorConfigBase.CompareByOrder);

            Interceptor[] interceptors = new Interceptor[configs.Count];

            for (int i = 0; i < configs.Count; ++i)
            {
                interceptors[i] = configs[i].CreateInterceptor(context);
            }

            return interceptors;
        }

    }

}
