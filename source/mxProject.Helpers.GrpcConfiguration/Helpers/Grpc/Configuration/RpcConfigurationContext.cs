using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace mxProject.Helpers.Grpc.Configuration
{

    /// <summary>
    /// The contextual information about gRPC configuration.
    /// </summary>
    public class RpcConfigurationContext : IDisposable
    {

        #region ctor

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="disposableRpcObjects">A value indicating whether the generated gRPC objects can be disposed.</param>
        public RpcConfigurationContext(RpcConfigurationConfig config, bool disposableRpcObjects = false)
        {
            DisposableRpcObjects = disposableRpcObjects;
            LoadConfig(config);
        }

        #endregion

        #region dtor

        /// <summary>
        /// Destructor.
        /// </summary>
        ~RpcConfigurationContext()
        {
            Dispose(false);
        }

        #endregion

        #region dispose

        /// <summary>
        /// Gets a value indicating whether the generated gRPC objects can be disposed.
        /// </summary>
        public bool DisposableRpcObjects { get; }

        private bool m_Disposed = false;
        
        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (DisposableRpcObjects)
            {
                DisposeFactory(m_ChannelActivators.Values);
                DisposeFactory(m_CallInvokerActivators.Values);
                DisposeFactory(m_ServerPortActivators.Values);
                DisposeFactory(m_InterceptorActivators.Values);
                DisposeFactory(m_ServerCredentialsActivators.Values);
                DisposeFactory(m_ChannelCredentialsActivators.Values);
            }
            m_Disposed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ThrowExceptionIfDisposed()
        {
            if (m_Disposed) { throw new ObjectDisposedException("RpcConfigurationContext"); }
        }

        #endregion

        #region config

        /// <summary>
        /// Load the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        private void LoadConfig(RpcConfigurationConfig config)
        {

            // channel

            m_ChannelActivators.Clear();
            m_ServerPortActivators.Clear();

            foreach (var channelConfig in RpcConfigurationUtility.EnumerateAll(config.Channels))
            {
                if (string.IsNullOrEmpty(channelConfig.Name)) { throw new RpcConfigurationException("The channel name is not set."); }
                m_ChannelActivators.Add(RpcConfigurationUtility.NormalizeName(channelConfig.Name), new Factory<Channel>(() => CreateChannel(channelConfig, this), DisposeChannel));
                m_ServerPortActivators.Add(RpcConfigurationUtility.NormalizeName(channelConfig.Name), new Factory<ServerPort>(() => CreateServerPort(channelConfig, this), DisposeServerPort));
            }

            // credential

            m_ChannelCredentialsActivators.Clear();
            m_ServerCredentialsActivators.Clear();

            foreach (var credentialsConfig in RpcConfigurationUtility.EnumerateAll(config.Credentials, config.ExtraCredentials))
            {
                if (string.IsNullOrEmpty(credentialsConfig.Name)) { throw new RpcConfigurationException("The credential name is not set."); }
                m_ChannelCredentialsActivators.Add(RpcConfigurationUtility.NormalizeName(credentialsConfig.Name), new Factory<ChannelCredentials>(() => CreateChannelCredentials(credentialsConfig, this), null));
                m_ServerCredentialsActivators.Add(RpcConfigurationUtility.NormalizeName(credentialsConfig.Name), new Factory<ServerCredentials>(() => CreateServerCredentials(credentialsConfig, this), null));
            }

            // interceptor

            m_InterceptorActivators.Clear();

            foreach (var interceptorConfig in RpcConfigurationUtility.EnumerateAll(config.Interceptors, config.ExtraInterceptors))
            {
                if (interceptorConfig is Interceptors.InterceptorReference) { continue; }
                if (string.IsNullOrEmpty(interceptorConfig.Name)) { throw new RpcConfigurationException("The interceptor name is not set."); }
                m_InterceptorActivators.Add(RpcConfigurationUtility.NormalizeName(interceptorConfig.Name), new Factory<Interceptor>(() => CreateInterceptor(interceptorConfig, this), DisposeInterceptor));
            }

            // callInvoker

            m_CallInvokerActivators.Clear();

            foreach (var invokerConfig in RpcConfigurationUtility.EnumerateAll(config.CallInvokers, config.ExtraCallInvokers))
            {
                if (string.IsNullOrEmpty(invokerConfig.Name)) { throw new RpcConfigurationException("The callInvoker name is not set."); }
                m_CallInvokerActivators.Add(RpcConfigurationUtility.NormalizeName(invokerConfig.Name), new Factory<CallInvoker>(() => CreateCallInvoker(invokerConfig, this), DisposeCallInvoker));
            }

            // service

            m_ServiceConfigs.Clear();

            foreach (var serviceConfig in RpcConfigurationUtility.EnumerateAll(config.Services))
            {
                if (string.IsNullOrEmpty(serviceConfig.Name)) { throw new RpcConfigurationException("The service name is not set."); }
                m_ServiceConfigs.Add(RpcConfigurationUtility.NormalizeName(serviceConfig.Name), serviceConfig);
            }

        }

        #endregion

        #region service

        private readonly Dictionary<string, RpcServiceConfig> m_ServiceConfigs = new Dictionary<string, RpcServiceConfig>();

        /// <summary>
        /// Intercept the service.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="serviceName">The service name on the configuration.</param>
        /// <returns></returns>
        public ServerServiceDefinition Intercept(ServerServiceDefinition service, string serviceName)
        {
            return GetServiceConfig(serviceName).Intercept(service, this);
        }

        /// <summary>
        /// Gets the service config.
        /// </summary>
        /// <param name="name">The service name in the configuration.</param>
        /// <returns>The config.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        protected RpcServiceConfig GetServiceConfig(string name)
        {
            if (TryGetServiceConfig(name, out RpcServiceConfig config)) { return config; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the service config.
        /// </summary>
        /// <param name="name">The service name in the configuration.</param>
        /// <param name="config">The config.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        protected bool TryGetServiceConfig(string name, out RpcServiceConfig config)
        {
            return m_ServiceConfigs.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out config);
        }

        #endregion

        #region channel

        private readonly Dictionary<string, Factory<Channel>> m_ChannelActivators = new Dictionary<string, Factory<Channel>>();

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <param name="name">The channel name in the configuration.</param>
        /// <returns>The channel.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public Channel GetChannel(string name)
        {
            if (TryGetChannel(name, out Channel channel)) { return channel; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <param name="name">The channel name in the configuration.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetChannel(string name, out Channel channel)
        {
            if (!m_ChannelActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<Channel> factory))
            {
                channel = null;
                return false;
            }
            channel = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="config">The channel config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual Channel CreateChannel(RpcChannelConfig config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateChannel(context);
        }

        /// <summary>
        /// Dispose the channel.
        /// </summary>
        /// <param name="channel"></param>
        private void DisposeChannel(Channel channel)
        {
            if (!DisposableRpcObjects) { return; }
            if (channel == null) { return; }
            if (channel.State != ChannelState.Shutdown)
            {
                try
                {
                    channel.ShutdownAsync().Wait();
                }
                catch( AggregateException ex)
                {
                    foreach( Exception inner in ex.InnerExceptions)
                    {
                        System.Diagnostics.Trace.WriteLine(inner.Message);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region serverPort

        private readonly Dictionary<string, Factory<ServerPort>> m_ServerPortActivators = new Dictionary<string, Factory<ServerPort>>();

        /// <summary>
        /// Gets the serverPort.
        /// </summary>
        /// <param name="name">The channel name in the configuration.</param>
        /// <returns>The serverPort.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public ServerPort GetServerPort(string name)
        {
            if (TryGetServerPort(name, out ServerPort serverPort)) { return serverPort; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the serverPort.
        /// </summary>
        /// <param name="name">The channel name in the configuration.</param>
        /// <param name="serverPort">The serverPort.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetServerPort(string name, out ServerPort serverPort)
        {
            if (!m_ServerPortActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<ServerPort> factory))
            {
                serverPort = null;
                return false;
            }
            serverPort = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new serverPort.
        /// </summary>
        /// <param name="config">The channel config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual ServerPort CreateServerPort(RpcChannelConfig config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateServerPort(context);
        }

        /// <summary>
        /// Dispose the serverPort.
        /// </summary>
        /// <param name="serverPort"></param>
        private void DisposeServerPort(ServerPort serverPort)
        {
            if (!DisposableRpcObjects) { return; }
        }

        #endregion

        #region callInvoker

        private readonly Dictionary<string, Factory<CallInvoker>> m_CallInvokerActivators = new Dictionary<string, Factory<CallInvoker>>();

        /// <summary>
        /// Gets the callInvoker.
        /// </summary>
        /// <param name="name">The callInvoker name in the configuration.</param>
        /// <returns>The callInvoker.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public CallInvoker GetCallInvoker(string name)
        {
            if (TryGetCallInvoker(name, out CallInvoker callInvoker)) { return callInvoker; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the callInvoker.
        /// </summary>
        /// <param name="name">The callInvoker name in the configuration.</param>
        /// <param name="callInvoker">The callInvoker.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetCallInvoker(string name, out CallInvoker callInvoker)
        {
            if (!m_CallInvokerActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<CallInvoker> factory))
            {
                callInvoker = null;
                return false;
            }
            callInvoker = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new callInvoker.
        /// </summary>
        /// <param name="config">The callInvoker config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual CallInvoker CreateCallInvoker(RpcCallInvokerConfigBase config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateCallInvoker(context.GetChannel(config.ChannelName), context);
        }

        /// <summary>
        /// Dispose the callInvoker.
        /// </summary>
        /// <param name="callInvoker"></param>
        private void DisposeCallInvoker(CallInvoker callInvoker)
        {
            if (!DisposableRpcObjects) { return; }
            IDisposable disposable = callInvoker as IDisposable;
            disposable?.Dispose();
        }

        #endregion

        #region interceptor

        private readonly Dictionary<string, Factory<Interceptor>> m_InterceptorActivators = new Dictionary<string, Factory<Interceptor>>();

        /// <summary>
        /// Gets the interceptor.
        /// </summary>
        /// <param name="name">The interceptor name in the configuration.</param>
        /// <returns>The interceptor.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public Interceptor GetInterceptor(string name)
        {
            if (TryGetInterceptor(name, out Interceptor interceptor)) { return interceptor; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the interceptor.
        /// </summary>
        /// <param name="name">The interceptor name in the configuration.</param>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetInterceptor(string name, out Interceptor interceptor)
        {
            if (!m_InterceptorActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<Interceptor> factory))
            {
                interceptor = null;
                return false;
            }
            interceptor = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new interceptor.
        /// </summary>
        /// <param name="config">The interceptor config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual Interceptor CreateInterceptor(RpcInterceptorConfigBase config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateInterceptor(context);
        }

        /// <summary>
        /// Dispose the interceptor.
        /// </summary>
        /// <param name="intercepter"></param>
        private void DisposeInterceptor(Interceptor intercepter)
        {
            if (!DisposableRpcObjects) { return; }
            IDisposable disposable = intercepter as IDisposable;
            disposable?.Dispose();
        }

        #endregion

        #region credentials

        private readonly Dictionary<string, Factory<ChannelCredentials>> m_ChannelCredentialsActivators = new Dictionary<string, Factory<ChannelCredentials>>();
        private readonly Dictionary<string, Factory<ServerCredentials>> m_ServerCredentialsActivators = new Dictionary<string, Factory<ServerCredentials>>();

        /// <summary>
        /// Gets the channel credentials.
        /// </summary>
        /// <param name="name">The credentials name in the configuration.</param>
        /// <returns>The credentials.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public ChannelCredentials GetChannelCredentials(string name)
        {
            if (TryGetChannelCredentials(name, out ChannelCredentials credentials)) { return credentials; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the channel credentials.
        /// </summary>
        /// <param name="name">The credentials name in the configuration.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetChannelCredentials(string name, out ChannelCredentials credentials)
        {
            if (!m_ChannelCredentialsActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<ChannelCredentials> factory))
            {
                credentials = null;
                return false;
            }
            credentials = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new channel credentials.
        /// </summary>
        /// <param name="config">The credentials config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual ChannelCredentials CreateChannelCredentials(RpcCredentialsConfigBase config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateChannelCredentials(context);
        }

        /// <summary>
        /// Gets the server credentials.
        /// </summary>
        /// <param name="name">The credentials name in the configuration.</param>
        /// <returns>The credentials.</returns>
        /// <exception cref="RpcConfigurationException">
        /// The specified name is not found.
        /// </exception>
        public ServerCredentials GetServerCredentials(string name)
        {
            if (TryGetServerCredentials(name, out ServerCredentials credentials)) { return credentials; }
            throw new RpcConfigurationException(string.Format("The specified name is not found. The name is '{0}'", name));
        }

        /// <summary>
        /// Gets the server credentials.
        /// </summary>
        /// <param name="name">The credentials name in the configuration.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>Returns true if can get; otherwise, false.</returns>
        public bool TryGetServerCredentials(string name, out ServerCredentials credentials)
        {
            if (!m_ServerCredentialsActivators.TryGetValue(RpcConfigurationUtility.NormalizeName(name), out Factory<ServerCredentials> factory))
            {
                credentials = null;
                return false;
            }
            credentials = factory.GetObject();
            return true;
        }

        /// <summary>
        /// Create a new server credentials.
        /// </summary>
        /// <param name="config">The credentials config.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">
        /// This instance has already been disposed.
        /// </exception>
        protected virtual ServerCredentials CreateServerCredentials(RpcCredentialsConfigBase config, RpcConfigurationContext context)
        {
            ThrowExceptionIfDisposed();
            return config.CreateServerCredentials(context);
        }

        #endregion

        #region certificates

        /// <summary>
        /// Gets the root certificates.
        /// </summary>
        /// <param name="configValue">The value in the configuration.</param>
        /// <returns>
        /// <para>
        /// This method calls the method set in <see cref="RootCertificatesGetter"/> property to get the certificates.
        /// If the value of <see cref="RootCertificatesGetter"/> property is null, this method regards <paramref name="configValue"/> as a file path and returns the contents of that file.
        /// </para>
        /// </returns>
        protected internal virtual string GetRootCertificates(string configValue)
        {
            if (string.IsNullOrEmpty(configValue)) { return null; }
            return (RootCertificatesGetter ?? GetFileData)(configValue);
        }

        /// <summary>
        /// Gets the certificate chain.
        /// </summary>
        /// <param name="configValue">The value in the configuration.</param>
        /// <returns>
        /// <para>
        /// This method calls the method set in <see cref="CertificateChainGetter"/> property to get the certificates.
        /// If the value of <see cref="CertificateChainGetter"/> property is null, this method regards <paramref name="configValue"/> as a file path and returns the contents of that file.
        /// </para>
        /// </returns>
        protected internal virtual string GetCertificateChain(string configValue)
        {
            if (string.IsNullOrEmpty(configValue)) { return null; }
            return (CertificateChainGetter ?? GetFileData)(configValue);
        }

        /// <summary>
        /// Gets the private key.
        /// </summary>
        /// <param name="configValue">The value in the configuration.</param>
        /// <returns>
        /// <para>
        /// This method calls the method set in <see cref="PrivateKeyGetter"/> property to get the certificates.
        /// If the value of <see cref="PrivateKeyGetter"/> property is null, this method regards <paramref name="configValue"/> as a file path and returns the contents of that file.
        /// </para>
        /// </returns>
        protected internal virtual string GetPrivateKey(string configValue)
        {
            if (string.IsNullOrEmpty(configValue)) { return null; }
            return (PrivateKeyGetter ?? GetFileData)(configValue);
        }

        /// <summary>
        /// Gets or sets the method to get PEM encoding of the server root certificates.
        /// </summary>
        public static Func<string, string> RootCertificatesGetter { get; set; }

        /// <summary>
        /// Gets or sets the method to get PEM encoded certificate chain.
        /// </summary>
        public static Func<string, string> CertificateChainGetter { get; set; }

        /// <summary>
        /// Gets or sets the method to get PEM encoded private key.
        /// </summary>
        public static Func<string, string> PrivateKeyGetter { get; set; }

        /// <summary>
        /// Gets the contents of the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private static string GetFileData(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        #endregion

        #region factory

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factories"></param>
        private void DisposeFactory<T>(IEnumerable<Factory<T>> factories) where T : class
        {
            if (factories == null) { return; }

            foreach (Factory<T> factory in factories)
            {
                try
                {
                    factory.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class Factory<T> : IDisposable where T : class
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activator"></param>
            /// <param name="onDispose"></param>
            internal Factory(Func<T> activator, Action<T> onDispose)
            {
                m_Activator = activator;
                m_OnDispose = onDispose;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
                if (m_Cache != null) { m_OnDispose?.Invoke(m_Cache); }
            }

            private readonly Func<T> m_Activator;
            private readonly Action<T> m_OnDispose;
            private T m_Cache;

            /// <summary>
            /// 
            /// </summary>
            internal T GetObject()
            {
                if (m_Cache == null)
                {
                    lock (this)
                    {
                        if (m_Cache == null)
                        {
                            m_Cache = m_Activator();
                        }
                    }
                }
                return m_Cache;
            }

        }

        #endregion

    }

}
