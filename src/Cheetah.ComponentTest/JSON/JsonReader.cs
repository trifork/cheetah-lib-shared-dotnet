using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Cheetah.ComponentTest.JSON
{
    public class JsonConfiguration
    {
        public string? ProducerTopic { get; set; }
        public string? ConsumerTopic { get; set; }
        public List<string>? ProducerData { get; set; }
        public List<string>? ConsumerData { get; set; }
    }

    public class JsonReader
    {
        const string FILE_PATH = "FOLDERPATH";

        IConfiguration? Configuration { get; }

        public JsonReader(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public JsonConfiguration ConsumeJson()
        {
            if (string.IsNullOrEmpty(FILE_PATH))
            {
                throw new InvalidDataException($"File path if not provided correct - {FILE_PATH}");
            }
            string filePath = Configuration.GetValue<string>(FILE_PATH);
            JsonConfiguration? config;
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Can't find file: {filePath}");
            }

            string jsonFile = File.ReadAllText(filePath);

            try
            {
                string jsonText = File.ReadAllText(jsonFile);
                config = JsonConvert.DeserializeObject<JsonConfiguration>(jsonText);

                if (config == null)
                {
                    throw new Exception("Could not deserialize file");
                }

                if (config.ProducerTopic == null)
                {
                    throw new Exception("Producer topic not set");
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing file {jsonFile}: {ex.Message}");
            }

            return config;
        }

    }
}