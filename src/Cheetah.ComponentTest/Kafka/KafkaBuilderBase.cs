namespace Cheetah.ComponentTest.Kafka;

public abstract class KafkaBuilder
{
    private ITokenService GetTokenService()
            {
                return new TestTokenService(
                    _configuration.GetValue<string>(KAFKA_CLIENTID_KEY),
                    _configuration.GetValue<string>(KAFKA_CLIENTSECRET_KEY),
                    _configuration.GetValue<string>(KAFKA_AUTH_ENDPOINT_KEY)
                );
            }
            
            private void ValidateConfiguration()
            {
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
    
                if (_isAvro)
                {
                    requiredKeys.Add(SCHEMA_REGISTRY_URL_KEY);
                }
                
                var missingKeys = requiredKeys.Where(key => string.IsNullOrWhiteSpace(_configuration.GetValue<string>(key))).ToList();
                if (missingKeys.Any())
                {
                    throw new ArgumentException($"Missing required configuration key(s): {string.Join(", ", missingKeys)}");
                }
            }
    
            private void ValidateKafkaUrlHasNoScheme()
            {
                var kafkaUrl = _configuration.GetValue<string>(KAFKA_URL_KEY);
                var hasSchemePrefix = Regex.Match(kafkaUrl, "(.+://).*");
                if (hasSchemePrefix.Success)
                {
                    throw new ArgumentException(
                        $"Found Kafka address: '{kafkaUrl}'. The Kafka URL cannot contain a scheme prefix - Remove the '{hasSchemePrefix.Groups[1].Value}'-prefix");
                }
            }
}
