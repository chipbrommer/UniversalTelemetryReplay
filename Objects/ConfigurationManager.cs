using Newtonsoft.Json;
using System.IO;

namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Default constructor</summary>
    /// <param name="file_path">Full path to the settings file</param>
    public class ConfigurationManager<T> where T : class, new()
    {
        private List<T> configurations;                // Config file template list
        private readonly string file_path;              // File path to the configurations file

        public ConfigurationManager(string file_path)
        {
            this.file_path = file_path;
            configurations = [];
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(file_path))
                {
                    return Save(); // Save the empty list to create the file
                }
                else
                {
                    configurations = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(file_path));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                string directoryName = Path.GetDirectoryName(file_path);
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(file_path, JsonConvert.SerializeObject(configurations, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configurations: {ex.Message}");
                return false;
            }
        }

        public List<T> GetData() 
        { 
            return configurations;
        }

        public void AddConfiguration(T configuration)
        {
            configurations.Add(configuration);
        }

        public bool RemoveConfiguration(int index)
        {
            if (configurations != null && index >= 0 && index < configurations.Count)
            {
                configurations.RemoveAt(index);
                return true;
            }

            return false;
        }
    }

}
