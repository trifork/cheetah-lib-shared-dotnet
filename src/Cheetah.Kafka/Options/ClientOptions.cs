using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Represents options for configuring a client.
    /// </summary>
    /// <typeparam name="TConfig">The type of configuration for the client.</typeparam>
    /// <typeparam name="TBuilder">The type of builder for the client.</typeparam>
    public class ClientOptions<TConfig, TBuilder> where TConfig : ClientConfig where TBuilder : class
    {
        internal Action<TConfig>? ConfigureAction { get; private set; }
        internal Action<TBuilder>? BuilderAction { get; private set; }

        /// <summary>
        /// Configures the client with the provided action.
        /// </summary>
        /// <param name="configureAction">The action to configure the client.</param>
        public void ConfigureClient(Action<TConfig> configureAction)
        {
            ConfigureAction = configureAction;
        }
        
        /// <summary>
        /// Configures the client builder with the provided action.
        /// </summary>
        /// <param name="builderAction">The action to configure the client builder.</param>
        public void ConfigureBuilder(Action<TBuilder> builderAction)
        {
            BuilderAction = builderAction;
        }
    }
}
