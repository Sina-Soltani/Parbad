// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class DependencyInjectionExtensions
    {
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType, ServiceLifetime lifetime)
        {
            services.Add(ServiceDescriptor.Describe(serviceType, serviceType, lifetime));

            return services;
        }

        public static IServiceCollection Add<TService>(this IServiceCollection services, ServiceLifetime lifetime) where TService : class
        {
            services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TService), lifetime));

            return services;
        }

        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));

            return services;
        }

        public static IServiceCollection Add<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            services.Add(ServiceDescriptor.Describe(typeof(TService), factory, lifetime));

            return services;
        }

        public static IServiceCollection TryAdd(this IServiceCollection services, Type serviceType, ServiceLifetime lifetime)
        {
            services.TryAdd(ServiceDescriptor.Describe(serviceType, serviceType, lifetime));

            return services;
        }

        public static IServiceCollection TryAdd<TService>(this IServiceCollection services, ServiceLifetime lifetime) where TService : class
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(TService), typeof(TService), lifetime));

            return services;
        }

        public static IServiceCollection TryAdd<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));

            return services;
        }

        public static IServiceCollection TryAdd<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            services.TryAdd(ServiceDescriptor.Describe(typeof(TService), factory, lifetime));

            return services;
        }

        public static IServiceCollection AddOrUpdate<TService>(this IServiceCollection services, ServiceLifetime lifetime)
            where TService : class
        {
            return services
                .RemoveAll<TService>()
                .Add<TService>(lifetime);
        }

        public static IServiceCollection AddOrUpdate<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            return services
                .RemoveAll<TService>()
                .Add<TService, TImplementation>(lifetime);
        }

        public static IServiceCollection AddOrUpdate<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            return services
                .RemoveAll<TService>()
                .Add(factory, lifetime);
        }
    }
}
