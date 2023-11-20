using Cheetah.ConsoleTester.DataModel;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace Cheetah.ConsoleTester;

public class JsonParser
{
    public static JsonKafkaConfig ParseJsonConfig(string jsonContent)
    {
        try
        {
            JsonKafkaConfig? kafkaConfig = JsonConvert.DeserializeObject<JsonKafkaConfig>(jsonContent);

            if (kafkaConfig == null)
            {
                throw new InvalidOperationException("Failed to deserialize JSON into JsonKafkaConfig object.");
            }

            // Check if properties are set
            if (string.IsNullOrWhiteSpace(kafkaConfig.ClientId))
            {
                throw new InvalidOperationException("ClientId must be set in the JSON configuration.");
            }

            if(string.IsNullOrWhiteSpace(kafkaConfig.ClientSecret))
            {
                throw new InvalidOperationException("ClientSecret must be set in the JSON configuration.");
            }
            
            if(string.IsNullOrWhiteSpace(kafkaConfig.TokenEndpoint))
            {
                throw new InvalidOperationException("TokenEndpoint must be set in the JSON configuration.");
            }
            
            if(string.IsNullOrWhiteSpace(kafkaConfig.KafkaUrl))
            {
                throw new InvalidOperationException("KafkaUrl must be set in the JSON configuration.");
            }
            
            if(string.IsNullOrWhiteSpace(kafkaConfig.Topic))
            {
                throw new InvalidOperationException("Topic must be set in the JSON configuration.");
            }
            
            if(string.IsNullOrWhiteSpace(kafkaConfig.Data))
            {
                throw new InvalidOperationException("Data must be set in the JSON configuration.");
            }

            return kafkaConfig;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error deserializing JSON: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred: " + ex.Message, ex);
        }
    }
}
