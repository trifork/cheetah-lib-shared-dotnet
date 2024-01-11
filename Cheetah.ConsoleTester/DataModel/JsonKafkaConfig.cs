namespace Cheetah.ConsoleTester.DataModel;

public class JsonKafkaConfig
{
    public string? ClientId { get; set; }
    
    public string? ClientSecret { get; set; }
    
    public string? TokenEndpoint { get; set; }
    
    public string? KafkaUrl { get; set; }

    public string? Topic { get; set; }
    
    public List<string>? Messages { get; set; }
}
