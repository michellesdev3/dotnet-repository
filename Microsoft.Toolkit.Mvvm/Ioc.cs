﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

#nullable enable

namespace Microsoft.Toolkit.Mvvm
{
    /// <summary>
    /// An Inversion of Control container that can be used to register and access instances of types providing services.
    /// It is focused on performance, and offers the ability to register service providers either through eagerly
    /// produced instances, or by using the factory pattern. The type is also fully thread-safe.
    /// In order to use this helper, first define a service and a service provider, like so:
    /// <code>
    /// public interface ILogger
    /// {
    ///     void Log(string text);
    /// }
    ///
    /// public class ConsoleLogger : ILogger
    /// {
    ///     void Log(string text) => Console.WriteLine(text);
    /// }
    /// </code>
    /// Then, register your services at startup, or at some time before using them:
    /// <code>
    /// IoC.Register&lt;ILogger, ConsoleLogger>();
    /// </code>
    /// Finally, use the <see cref="Ioc"/> type to retrieve service instances to use:
    /// <code>
    /// IoC.GetInstance&lt;ILogger>().Log("Hello world!");
    /// </code>
    /// The <see cref="Ioc"/> type will make sure to initialize your service instance if needed, or it will
    /// throw an exception in case the requested service has not been registered yet.
    /// </summary>
    public static class Ioc
    {
        /// <summary>
        /// The collection of currently registered service types.
        /// </summary>
        /// <remarks>
        /// This list is not used when retrieving registered instances through
        /// the <see cref="GetInstance{TService}"/> method, so it has no impact on
        /// performances there. This is only used to allow users to retrieve all
        /// the registered instances at any given time, or to unregister them all.
        /// </remarks>
        private static readonly HashSet<Type> RegisteredTypes = new HashSet<Type>();

        /// <summary>
        /// Checks whether or not a service of type <typeparamref name="TService"/> has already been registered.
        /// </summary>
        /// <typeparam name="TService">The type of service to check for registration.</typeparam>
        /// <returns><see langword="true"/> if the service <typeparamref name="TService"/> has already been registered, <see langword="false"/> otherwise.</returns>
        [Pure]
        public static bool IsRegistered<TService>()
            where TService : class
        {
            lock (Container<TService>.Lock)
            {
                return
                    !(Container<TService>.Instance is null) ||
                    !(Container<TService>.Factory is null);
            }
        }

        /// <summary>
        /// Registers a singleton instance of service <typeparamref name="TService"/> through the type <typeparamref name="TProvider"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <typeparam name="TProvider">The type of service provider for type <typeparamref name="TService"/> to register.</typeparam>
        /// <remarks>This method will create a new <typeparamref name="TProvider"/> instance for future use.</remarks>
        public static void Register<TService, TProvider>()
            where TService : class
            where TProvider : class, TService, new()
        {
            Register<TService>(new TProvider());
        }

        /// <summary>
        /// Registers a singleton instance of service <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <param name="provider">The <typeparamref name="TService"/> instance to register.</param>
        public static void Register<TService>(TService provider)
            where TService : class
        {
            lock (RegisteredTypes)
            lock (Container<TService>.Lock)
            {
                RegisteredTypes.Add(typeof(TService));

                Container<TService>.Factory = null;
                Container<TService>.Instance = provider;
            }
        }

        /// <summary>
        /// Registers a service <typeparamref name="TService"/> through a <see cref="Func{TResult}"/> factory.
        /// </summary>
        /// <typeparam name="TService">The type of service to register.</typeparam>
        /// <param name="factory">The factory of instances implementing the <typeparamref name="TService"/> service to use.</param>
        public static void Register<TService>(Func<TService> factory)
            where TService : class
        {
            lock (RegisteredTypes)
            lock (Container<TService>.Lock)
            {
                RegisteredTypes.Add(typeof(TService));

                Container<TService>.Factory = factory;
                Container<TService>.Instance = null;
            }
        }

        /// <summary>
        /// Unregisters a service of a specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service to unregister.</typeparam>
        public static void Unregister<TService>()
            where TService : class
        {
            lock (RegisteredTypes)
            lock (Container<TService>.Lock)
            {
                RegisteredTypes.Remove(typeof(TService));

                Container<TService>.Factory = null;
                Container<TService>.Instance = null;
            }
        }

        /// <summary>
        /// Resets the internal state of the <see cref="Ioc"/> type and unregisters all services.
        /// </summary>
        public static void Reset()
        {
            lock (RegisteredTypes)
            {
                foreach (Type serviceType in RegisteredTypes)
                {
                    Type containerType = typeof(Container<>).MakeGenericType(serviceType);
                    FieldInfo
                        lockField = containerType.GetField(nameof(Container<object>.Lock)),
                        lazyField = containerType.GetField(nameof(Container<object>.Factory)),
                        instanceField = containerType.GetField(nameof(Container<object>.Instance));

                    lock (lockField.GetValue(null))
                    {
                        lazyField.SetValue(null, null);
                        instanceField.SetValue(null, null);
                    }
                }

                RegisteredTypes.Clear();
            }
        }

        /// <summary>
        /// Gets all the currently registered services.
        /// </summary>
        /// <returns>A collection of all the currently registered services.</returns>
        /// <remarks>
        /// This will also cause the <see cref="Ioc"/> service to resolve instances of
        /// registered services that were setup to use lazy initialization.
        /// </remarks>
        [Pure]
        public static IReadOnlyCollection<object> GetAllServices()
        {
            lock (RegisteredTypes)
            {
                return (
                    from Type serviceType in RegisteredTypes
                    let resolver = typeof(Ioc).GetMethod(nameof(GetInstance))
                    let resolverOfT = resolver.MakeGenericMethod(serviceType)
                    let service = resolverOfT.Invoke(null, null)
                    select service).ToArray();
            }
        }

        /// <summary>
        /// Gets all the currently registered and instantiated services.
        /// </summary>
        /// <returns>A collection of all the currently registered and instantiated services.</returns>
        [Pure]
        public static IReadOnlyCollection<object> GetAllCreatedServices()
        {
            lock (RegisteredTypes)
            {
                return (
                    from Type serviceType in RegisteredTypes
                    let containerType = typeof(Container<>).MakeGenericType(serviceType)
                    let field = containerType.GetField(nameof(Container<object>.Instance))
                    let instance = field.GetValue(null)
                    where !(instance is null)
                    select instance).ToArray();
            }
        }

        /// <summary>
        /// Tries to resolve an instance of a registered type implementing the <typeparamref name="TService"/> service.
        /// </summary>
        /// <typeparam name="TService">The type of service to look for.</typeparam>
        /// <returns>An instance of a registered type implementing <typeparamref name="TService"/>, if registered.</returns>
        [Pure]
        public static bool TryGetInstance<TService>(out TService? service)
            where TService : class
        {
            lock (Container<TService>.Lock)
            {
                service = Container<TService>.Instance;

                if (!(service is null))
                {
                    return true;
                }

                Func<TService>? factory = Container<TService>.Factory;

                if (factory is null)
                {
                    return false;
                }

                service = Container<TService>.Instance = factory();

                return true;
            }
        }

        /// <summary>
        /// Resolves an instance of a registered type implementing the <typeparamref name="TService"/> service.
        /// </summary>
        /// <typeparam name="TService">The type of service to look for.</typeparam>
        /// <returns>An instance of a registered type implementing <typeparamref name="TService"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService GetInstance<TService>()
            where TService : class
        {
            TService? service = Container<TService>.Instance;

            if (service is null)
            {
                return GetInstanceOrThrow<TService>();
            }

            return service;
        }

        /// <summary>
        /// Tries to resolve a <typeparamref name="TService"/> instance with additional checks.
        /// This method implements the slow path for <see cref="GetInstance{TService}"/>, locking
        /// to ensure thread-safety and invoking the available factory, if possible.
        /// </summary>
        /// <typeparam name="TService">The type of service to look for.</typeparam>
        /// <returns>An instance of a registered type implementing <typeparamref name="TService"/>.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TService GetInstanceOrThrow<TService>()
            where TService : class
        {
            lock (Container<TService>.Lock)
            {
                TService? service = Container<TService>.Instance;

                // Check the instance field again for race conditions
                if (!(service is null))
                {
                    return service;
                }

                Func<TService>? factory = Container<TService>.Factory;

                // If no factory is available, the service hasn't been registered yet
                if (factory is null)
                {
                    throw new InvalidOperationException($"Service {typeof(TService)} not initialized");
                }

                return Container<TService>.Instance = factory();
            }
        }

        /// <summary>
        /// Creates an instance of a registered type implementing the <typeparamref name="TService"/> service.
        /// </summary>
        /// <typeparam name="TService">The type of service to look for.</typeparam>
        /// <returns>A new instance of a registered type implementing <typeparamref name="TService"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService GetInstanceWithoutCaching<TService>()
            where TService : class
        {
            lock (Container<TService>.Lock)
            {
                Func<TService>? factory = Container<TService>.Factory;

                if (!(factory is null))
                {
                    return factory();
                }

                TService? service = Container<TService>.Instance;

                if (!(service is null))
                {
                    Type serviceType = service.GetType();
                    Expression[] expressions = { Expression.New(serviceType) };
                    Expression body = Expression.Block(serviceType, expressions);
                    factory = Expression.Lambda<Func<TService>>(body).Compile();

                    Container<TService>.Factory = factory;

                    return factory();
                }

                throw new InvalidOperationException($"Service {typeof(TService)} not initialized");
            }
        }

        /// <summary>
        /// Internal container type for services of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of services to store in this type.</typeparam>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401", Justification = "Public fields for performance reasons")]
        private static class Container<T>
            where T : class
        {
            /// <summary>
            /// An <see cref="object"/> used to synchronize accesses to the <see cref="Factory"/> and <see cref="Instance"/> fields.
            /// </summary>
            public static readonly object Lock = new object();

            /// <summary>
            /// The optional <see cref="Func{T}"/> instance to produce instances of the service <typeparamref name="T"/>.
            /// </summary>
            public static Func<T>? Factory;

            /// <summary>
            /// The current <typeparamref name="T"/> instance being produced, if available.
            /// </summary>
            public static T? Instance;
        }
    }
}
