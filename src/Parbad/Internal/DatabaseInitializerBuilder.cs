using Microsoft.Extensions.DependencyInjection;
using Parbad.Data.Initializers;
using System;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DatabaseInitializerBuilder : IDatabaseInitializerBuilder
    {
        /// <summary>
        /// Initializes an instance of <see cref="DatabaseInitializerBuilder"/> class.
        /// </summary>
        /// <param name="services"></param>
        public DatabaseInitializerBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IDatabaseInitializerBuilder AddInitializer<TInitializer>(
            ServiceLifetime initializerLifetime)
            where TInitializer : class, IDatabaseInitializer
        {
            Services.Add<IDatabaseInitializer, TInitializer>(initializerLifetime);

            return this;
        }

        /// <inheritdoc />
        public IDatabaseInitializerBuilder AddInitializer(
            Func<IServiceProvider, IDatabaseInitializer> implementationFactory,
            ServiceLifetime initializerLifetime = ServiceLifetime.Transient)
        {
            Services.Add(implementationFactory, initializerLifetime);

            return this;
        }
    }
}
