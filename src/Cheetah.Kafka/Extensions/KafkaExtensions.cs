using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.Extensions
{
    /*
     We need to supply extension methods for each type of builder, and for both asynchronous and synchronous token functions.
     This results in 6 public methods, which all eventually call the same private method.
    */

    /// <summary>
    /// Extension methods for adding Cheetah OAuth2 authentication to Kafka clients.
    /// </summary>
    ///
    public static class KafkaExtensions
    {
        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka consumer.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="asyncTokenRequestFunc">A function which returns a Task, which results in a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <typeparam name="TKey">The key type on the builder</typeparam>
        /// <typeparam name="TValue">The value type on the builder</typeparam>
        /// <returns>The builder for method chaining</returns>
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ConsumerBuilder<TKey, TValue> builder,
            Func<
                Task<(string AccessToken, long Expiration, string Principal)>
            > asyncTokenRequestFunc,
            ILogger logger
        )
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }

        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka producer.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="asyncTokenRequestFunc">A function which returns a Task, which results in a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <typeparam name="TKey">The key type on the builder</typeparam>
        /// <typeparam name="TValue">The value type on the builder</typeparam>
        /// <returns>The builder for method chaining</returns>
        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ProducerBuilder<TKey, TValue> builder,
            Func<
                Task<(string AccessToken, long Expiration, string Principal)>
            > asyncTokenRequestFunc,
            ILogger logger
        )
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }

        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka admin client.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="asyncTokenRequestFunc">A function which returns a Task, which results in a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <returns>The builder for method chaining</returns>
        public static AdminClientBuilder AddCheetahOAuthentication(
            this AdminClientBuilder builder,
            Func<
                Task<(string AccessToken, long Expiration, string Principal)>
            > asyncTokenRequestFunc,
            ILogger logger
        )
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }

        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka consumer.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="tokenRequestFunc">A function which returns a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <typeparam name="TKey">The key type on the builder</typeparam>
        /// <typeparam name="TValue">The value type on the builder</typeparam>
        /// <returns>The builder for method chaining</returns>
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ConsumerBuilder<TKey, TValue> builder,
            Func<(string AccessToken, long Expiration, string Principal)> tokenRequestFunc,
            ILogger logger
        )
        {
            return builder.SetOAuthBearerTokenRefreshHandler(
                GetTokenRefreshHandler(tokenRequestFunc, logger)
            );
        }

        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka producer.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="tokenRequestFunc">A function which returns a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <typeparam name="TKey">The key type on the builder</typeparam>
        /// <typeparam name="TValue">The value type on the builder</typeparam>
        /// <returns>The builder for method chaining</returns>
        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ProducerBuilder<TKey, TValue> builder,
            Func<(string AccessToken, long Expiration, string Principal)> tokenRequestFunc,
            ILogger logger
        )
        {
            return builder.SetOAuthBearerTokenRefreshHandler(
                GetTokenRefreshHandler(tokenRequestFunc, logger)
            );
        }

        /// <summary>
        /// Adds Cheetah OAuth2 authentication to a Kafka admin client.
        /// </summary>
        /// <param name="builder">The builder to call this method on</param>
        /// <param name="tokenRequestFunc">A function which returns a tuple containing a token, expiration and optional principal name</param>
        /// <param name="logger">The logger to use when logging token-related messages</param>
        /// <returns>The builder for method chaining</returns>
        public static AdminClientBuilder AddCheetahOAuthentication(
            this AdminClientBuilder builder,
            Func<(string AccessToken, long Expiration, string Principal)> tokenRequestFunc,
            ILogger logger
        )
        {
            return builder.SetOAuthBearerTokenRefreshHandler(
                GetTokenRefreshHandler(tokenRequestFunc, logger)
            );
        }

        // Convenience to avoid spreading "GetAwaiter().GetResult()"
        private static Func<T> Synchronize<T>(Func<Task<T>> asyncTokenRequestFunc)
        {
            return () => asyncTokenRequestFunc().GetAwaiter().GetResult();
        }

        // Convenience to avoid spreading lambdas
        private static Action<IClient, string> GetTokenRefreshHandler(
            Func<(string AccessToken, long Expiration, string Principal)> func,
            ILogger logger
        )
        {
            return (client, _) => TokenRefreshHandler(client, func, logger);
        }

        private static void TokenRefreshHandler(
            IClient client,
            Func<(string AccessToken, long Expiration, string Principal)> tokenRequestFunc,
            ILogger logger
        )
        {
            try
            {
                var (AccessToken, Expiration, Principal) = tokenRequestFunc();

                if (string.IsNullOrWhiteSpace(AccessToken))
                {
                    SetFailure(
                        client,
                        logger,
                        "Supplied token function returned null or a valueless access token."
                    );
                    return;
                }

                client.OAuthBearerSetToken(AccessToken, Expiration, Principal);
            }
            catch (Exception ex)
            {
                SetFailure(
                    client,
                    logger,
                    $"Unhandled exception thrown when attempting to retrieve access token from IDP. {ex.GetType().Name}: {ex.Message}"
                );
                throw;
            }
        }

        private static void SetFailure(IClient client, ILogger logger, string errorMsg)
        {
            logger.LogError(errorMsg);
            client.OAuthBearerSetTokenFailure(errorMsg);
        }
    }
}
