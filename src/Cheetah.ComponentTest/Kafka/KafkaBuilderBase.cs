using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cheetah.ComponentTest.TokenService;
using Cheetah.Core.Infrastructure.Auth;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;

namespace Cheetah.ComponentTest.Kafka
{
    public abstract class KafkaBuilderBase
    {
        protected const string KAFKA_PREFIX = "KAFKA:";
        protected const string KAFKA_URL_KEY = KAFKA_PREFIX + "URL";
        protected const string KAFKA_CLIENTID_KEY = KAFKA_PREFIX + "CLIENTID";
        protected const string KAFKA_CLIENTSECRET_KEY = KAFKA_PREFIX + "CLIENTSECRET";
        protected const string KAFKA_AUTH_ENDPOINT_KEY = KAFKA_PREFIX + "AUTHENDPOINT";
        protected const string SCHEMA_REGISTRY_URL_KEY = KAFKA_PREFIX + "SCHEMAREGISTRYURL";

        protected IConfiguration? Configuration { get; }
        protected string? Topic { get; set; }
        protected bool IsAvro { get; private set; }
        protected SchemaRegistryConfig? SchemaRegistryConfig { get; private set; }

        protected KafkaBuilderBase(IConfiguration? configuration)
        {
            Configuration = configuration;
        }

        protected KafkaBuilderBase UsingAvroInternal(SchemaRegistryConfig? config = null)
        {
            SchemaRegistryConfig = config ?? new SchemaRegistryConfig
            {
                Url = Configuration.GetValue<string>(SCHEMA_REGISTRY_URL_KEY)
            };

            IsAvro = true;
            return this;
        }

        protected ITokenService GetTokenService()
        {
            return new TestTokenService(
                Configuration.GetValue<string>(KAFKA_CLIENTID_KEY),
                Configuration.GetValue<string>(KAFKA_CLIENTSECRET_KEY),
                Configuration.GetValue<string>(KAFKA_AUTH_ENDPOINT_KEY)
            );
        }

        protected void ValidateInput()
        {
            ValidateTopicHasValue();
            ValidateNoMissingKeys();
            ValidateKafkaUrlHasNoScheme();
        }

        private void ValidateNoMissingKeys()
        {
            var requiredKeys = new List<string>
        {
            KAFKA_URL_KEY,
            KAFKA_AUTH_ENDPOINT_KEY,
            KAFKA_CLIENTID_KEY,
            KAFKA_CLIENTSECRET_KEY
        };

            if (IsAvro)
            {
                requiredKeys.Add(SCHEMA_REGISTRY_URL_KEY);
            }

            var missingKeys = requiredKeys.Where(key => string.IsNullOrWhiteSpace(Configuration.GetValue<string>(key))).ToList();
            if (missingKeys.Any())
            {
                throw new ArgumentException($"Missing required configuration key(s): {string.Join(", ", missingKeys)}");
            }
        }

        private void ValidateKafkaUrlHasNoScheme()
        {
            // Kafka producers and consumers will silently fail if a scheme is prepended (e.g. http://kafka:19092)
            // This ensures that we fail early and loudly if this is the case. Uses a regex that should match any prefix followed by '://'
            // We could also just strip the scheme prefix if it's there, but that would hide the fact that the input is wrong.
            var kafkaUrl = Configuration.GetValue<string>(KAFKA_URL_KEY);
            var hasSchemePrefix = Regex.Match(kafkaUrl, "(.*://).*");
            if (hasSchemePrefix.Success)
            {
                throw new ArgumentException(
                    $"Found Kafka address: '{kafkaUrl}'. The Kafka URL cannot contain a scheme prefix - Remove the '{hasSchemePrefix.Groups[1].Value}'-prefix");
            }
        }

        private void ValidateTopicHasValue()
        {
            if (string.IsNullOrWhiteSpace(Topic))
            {
                throw new ArgumentException("A topic must be provided");
            }

            var hasOnlyValidCharacters = Regex.Match(Topic, "^[a-zA-Z0-9\\._\\-]+$");
            if (!hasOnlyValidCharacters.Success)
            {
                throw new ArgumentException($"Received topic with invalid characters '{Topic}'. Topic names can only contain alphanumeric characters, '.', '-' and '_'.");
            }

            if (Topic.Length > 249)
            {
                throw new ArgumentException("Topic names cannot exceed 249 characters");
            }
        }
    }
}
