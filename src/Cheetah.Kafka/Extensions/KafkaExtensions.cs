using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Cheetah.Kafka.Extensions
{
    public static class CheetahKafkaExtensions
    {
        // We need one for each, since the return type must match the builder and they share no common interface.
        // Allows supplying an asynchronous token function
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ConsumerBuilder<TKey, TValue> builder, 
            Func<Task<(string AccessToken, long Expiration, string? PrincipalName)?>> asyncTokenRequestFunc, 
            ILogger logger)
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }

        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ProducerBuilder<TKey, TValue> builder, 
            Func<Task<(string AccessToken, long Expiration, string? PrincipalName)?>> asyncTokenRequestFunc, 
            ILogger logger)
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }

        public static AdminClientBuilder AddCheetahOAuthentication(
            this AdminClientBuilder builder, 
            Func<Task<(string AccessToken, long Expiration, string? PrincipalName)?>> asyncTokenRequestFunc, 
            ILogger logger)
        {
            return AddCheetahOAuthentication(builder, Synchronize(asyncTokenRequestFunc), logger);
        }
        
        // Allows supplying a synchronous token function
        
        public static ConsumerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ConsumerBuilder<TKey, TValue> builder, 
            Func<(string AccessToken, long Expiration, string? PrincipalName)?> tokenRequestFunc, 
            ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler(GetTokenRefreshHandler(tokenRequestFunc, logger));
        }

        public static ProducerBuilder<TKey, TValue> AddCheetahOAuthentication<TKey, TValue>(
            this ProducerBuilder<TKey, TValue> builder, 
            Func<(string AccessToken, long Expiration, string? PrincipalName)?> tokenRequestFunc, 
            ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler(GetTokenRefreshHandler(tokenRequestFunc, logger));
        }

        public static AdminClientBuilder AddCheetahOAuthentication(
            this AdminClientBuilder builder, 
            Func<(string AccessToken, long Expiration, string? PrincipalName)?> tokenRequestFunc, 
            ILogger logger)
        {
            return builder.SetOAuthBearerTokenRefreshHandler(GetTokenRefreshHandler(tokenRequestFunc, logger));
        }

        // Convenience to avoid spreading "GetAwaiter().GetResult()"
        private static Func<T> Synchronize<T>(Func<Task<T>> asyncTokenRequestFunc) => 
            () => asyncTokenRequestFunc().GetAwaiter().GetResult();

        // Convenience to avoid spreading lambdas
        private static Action<IClient, string> GetTokenRefreshHandler(Func<(string AccessToken, long Expiration, string? PrincipalName)?> func, ILogger logger) =>
            (client, _) => TokenRefreshHandler(client, func, logger); 

        private static void TokenRefreshHandler(IClient client, Func<(string AccessToken, long Expiration, string? PrincipalName)?> tokenRequestFunc, ILogger logger)
        {
            try
            {
                var token = tokenRequestFunc();
                if (token == null || string.IsNullOrWhiteSpace(token.Value.AccessToken))
                {
                    SetFailure(client, logger, "Supplied token function returned null or a valueless access token.");
                    return;
                }
                
                client.OAuthBearerSetToken(token.Value.AccessToken, token.Value.Expiration, token.Value.PrincipalName);
            }
            catch (Exception ex)
            {
                SetFailure(client, logger, $"Unhandled exception thrown when attempting to retrieve access token from IDP. {ex.GetType().Name}: {ex.Message}");
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
