using System;
using System.Collections.Generic;
using System.Linq;
using Cheetah.OpenSearch.Configuration;
using Cheetah.OpenSearch.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using OpenSearch.Client;
using Xunit;

namespace Cheetah.OpenSearch.Test.DependencyInjection;

[Trait("Category", "OpenSearch"), Trait("TestType", "Unit")]
public class DependencyInjectionTests
{
    public static IEnumerable<object[]> RequiredConfigurationTestCases()
        {
            yield return new object[] {
                OpenSearchConfig.OpenSearchAuthMode.None,
                new List<KeyValuePair<string, string?>>
                {
                    new ("OPENSEARCH:URL", "http://localhost:9200")
                }};
            
            yield return new object[] {
                OpenSearchConfig.OpenSearchAuthMode.Basic,
                new List<KeyValuePair<string, string?>>
                {
                    new ("OPENSEARCH:URL", "http://localhost:9200"),
                    new ("OPENSEARCH:USERNAME", "admin"),
                    new ("OPENSEARCH:PASSWORD", "admin"),
                }};

            yield return new object[] {
                OpenSearchConfig.OpenSearchAuthMode.OAuth2,
                new List<KeyValuePair<string, string?>>
                {
                    new ("OPENSEARCH:URL", "http://localhost:9200"),
                    new ("OPENSEARCH:OAUTH2:CLIENTID", "clientId"),
                    new ("OPENSEARCH:OAUTH2:CLIENTSECRET", "1234"),
                    new ("OPENSEARCH:OAUTH2:TOKENENDPOINT", "http://localhost:1752/oauth2/token")
                }};
        }

        [Theory]
        [MemberData(nameof(RequiredConfigurationTestCases))]
        public void Should_NotThrowExceptionDuringInitialization_When_AllRequiredConfigurationIsPresent(OpenSearchConfig.OpenSearchAuthMode authMode, List<KeyValuePair<string, string?>> requiredConfiguration)
        {
            // To be able to reuse the test cases, we explicitly add the auth mode seperately.
            // This is due to the fact that while the AuthMode is required, it also changes the remaining required configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(requiredConfiguration
                    .Append(new ("OPENSEARCH:AUTHMODE", authMode.ToString()))
                ).Build();
            
            var setupAction = new Action(() =>
            {
                var serviceProvider = CreateServiceProvider(configuration);
                serviceProvider.GetRequiredService<IOpenSearchClient>();
            });
            
            setupAction.Invoking(x => x())
                .Should()
                .NotThrow("because we should be able to instantiate a client when all required configuration is set");
        }

        public static IEnumerable<object[]> MissingRequiredKeyTestCases()
        {
            // For all positive test cases
            foreach(var testCase in RequiredConfigurationTestCases())
            {
                // Obtain the test input
                var authMode = (OpenSearchConfig.OpenSearchAuthMode) testCase[0];
                var requiredConfigurations = (List<KeyValuePair<string, string?>>) testCase[1];
                
                // For each key in the test input
                foreach(var configuration in requiredConfigurations)
                {
                    // Generate a new test case, where a single key is missing and the auth mode is added to configuration
                    yield return new object[]
                    {
                        configuration.Key,
                        requiredConfigurations
                            .Except(new[] { configuration })
                            .Append(new KeyValuePair<string, string?>("OPENSEARCH:AUTHMODE", authMode.ToString()))
                            .ToList()
                    };
                }
            }
        }
        
        [Theory]
        [MemberData(nameof(MissingRequiredKeyTestCases))]
        public void Should_ThrowExceptionDuringInitialization_When_SingleKeyIsMissingFromRequiredConfiguration(string missingKey, List<KeyValuePair<string, string?>> configuration)
        {
            var configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();
            
            // Simulate code that will register and DI an IOpenSearchClient
            var setupAction = new Action(() =>
            {
                var serviceProvider = CreateServiceProvider(configurationRoot);
                serviceProvider.GetRequiredService<IOpenSearchClient>();
            });
            
            setupAction
                .Invoking(x => x())
                .Should()
                .Throw<Exception>($"because we should not be able to instantiate a client when {missingKey} is not set");
        }
        
        private static ServiceProvider CreateServiceProvider(IConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IHostEnvironment>(new HostingEnvironment()
            {
                EnvironmentName = "Development"
            });
            serviceCollection.AddCheetahOpenSearch(config);
            return serviceCollection.BuildServiceProvider();
        }
}
