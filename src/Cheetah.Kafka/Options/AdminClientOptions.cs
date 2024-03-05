using System;
using Confluent.Kafka;

namespace Cheetah.Kafka
{
    public class AdminClientOptions : ClientOptions<AdminClientConfig, AdminClientBuilder>
    {
        
    }

    public class AdminClientOptionsBuilder : IOptionsBuilder<AdminClientOptions>
    {
        private readonly AdminClientOptions _options = new AdminClientOptions();

        public AdminClientOptionsBuilder ConfigureClient(Action<AdminClientConfig> configureAction)
        {
            _options.ConfigureClient(configureAction);
            return this;
        }
    
        public AdminClientOptionsBuilder ConfigureBuilder(Action<AdminClientBuilder> builderAction)
        {
            _options.ConfigureBuilder(builderAction);
            return this;
        }
    
        public AdminClientOptions Build(IServiceProvider serviceProvider)
        {
            return _options;
        }
    }
}
