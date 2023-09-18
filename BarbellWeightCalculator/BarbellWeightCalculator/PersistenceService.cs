using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace BarbellWeightCalculator
{
    public class PersistenceService
    {
        private readonly string _dataFilePath;

        public PersistenceService(string filePath, string fileName)
        {
            _dataFilePath = Path.Combine(filePath, fileName);
        }

        public WeightConfiguration ReadWeightConfiguration()
        {
            var emptyWeightConfiguration 
                = new WeightConfiguration(0, 0, true, new SortedDictionary<double, uint>());
            try
            {
                string json = File.ReadAllText(_dataFilePath);
                var weightConfiguration = JsonSerializer.Deserialize<WeightConfiguration>(json)
                    ?? emptyWeightConfiguration;
                return weightConfiguration;
            }
            catch
            {
                return emptyWeightConfiguration;
            }
        }

        public bool WriteWeightConfiguration(WeightConfiguration weightConfiguration)
        {
            string json = JsonSerializer.Serialize<WeightConfiguration>(weightConfiguration);
            File.WriteAllText(_dataFilePath, json);
            return true;
        }

    }
}
