using Cheetah.ConsoleTester.DataModel;
using Cheetah.ConsoleTester.Kafka;

namespace Cheetah.ConsoleTester;

internal class ConsoleTester
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: MyConsoleApp.exe <input_file>");
            return;
        }

        string inputFile = args[0];

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Input file does not exist.");
            return;
        }

        var jsonConfig = JsonParser.ParseJsonConfig(File.ReadAllText(inputFile));

        var kafkaWriter = new KafkaJsonWriter(jsonConfig);
        
        try
        { 
            kafkaWriter.WriteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}