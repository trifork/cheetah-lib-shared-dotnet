using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    /// <summary>
    /// Represents options for configuring an admin client.
    /// </summary>
    public class AdminClientOptions : ClientOptions<AdminClientConfig, AdminClientBuilder>
    {

    }

    /// <summary>
    /// Builder for configuring <see cref="AdminClientOptions"/>.
    /// </summary>
    public class AdminClientOptionsBuilder : IOptionsBuilder<AdminClientOptions>
    {
        private readonly AdminClientOptions _options = new AdminClientOptions();

        /// <summary>
        /// Configures the admin client with the provided action.
        /// </summary>
        /// <param name="configureAction">The action to configure the admin client.</param>
        /// <returns>The builder instance.</returns>
        public AdminClientOptionsBuilder ConfigureClient(Action<AdminClientConfig> configureAction)
        {
            _options.ConfigureClient(configureAction);
            return this;
        }

        /// <summary>
        /// Configures the admin client builder with the provided action.
        /// </summary>
        /// <param name="builderAction">The action to configure the admin client builder.</param>
        /// <returns>The builder instance.</returns>
        public AdminClientOptionsBuilder ConfigureBuilder(Action<AdminClientBuilder> builderAction)
        {
            _options.ConfigureBuilder(builderAction);
            return this;
        }

        /// <summary>
        /// Builds the admin client options.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The configured admin client options.</returns>
        public AdminClientOptions Build(IServiceProvider serviceProvider)
        {
            return _options;
        }
    }
}
