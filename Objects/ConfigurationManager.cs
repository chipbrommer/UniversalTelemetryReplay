using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Default constructor</summary>
    /// <param name="file_path">Full path to the settings file</param>
    public class ConfigurationManager<T>(string file_path) where T : class, new()
    {
        private ObservableCollection<T> configurations = [];    // Config file template list
        private readonly string file_path = file_path;          // File path to the configurations file

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
                    string fileContent = File.ReadAllText(file_path);
                    if (string.IsNullOrEmpty(fileContent))
                    {
                        return Save(); // Save the empty list to create the file
                    }
                    else
                    {
                        configurations = JsonConvert.DeserializeObject<ObservableCollection<T>>(fileContent);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                return false;
            }
        }

        public bool LoadConfigFromFile(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    string json = File.ReadAllText(filepath);
                    configurations = JsonConvert.DeserializeObject<ObservableCollection<T>>(json);
                    return Save();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations from file: {ex.Message}");
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

        public ObservableCollection<T> GetData()
        {
            return configurations;
        }

        public int GetNextConfigurationIndex()
        {
            return configurations.Count;
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

        public bool UpdateConfiguration(int index, T configuration)
        {
            if (configurations != null && index >= 0 && index < configurations.Count)
            {
                // Overwrite the item at the specified index with the new configuration
                configurations[index] = configuration;
                return true;
            }

            return false; // Configuration with the specified index not found
        }
    }
}
