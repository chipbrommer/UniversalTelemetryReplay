using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalTelemetryReplay.Objects
{
    public class ConfigurationManager<T> where T : class, new()
    {
        private List<T>? data;       // Config file template list
        private string file_path;   // File path to the configurations file

        /// <summary>Default constructor</summary>
        /// <param name="file_path">Full path to the settings file</param>
        public ConfigurationManager(string file_path)
        {
            this.file_path = file_path;
        }

        public List<T>? Load()
        {
            try
            {
                if (!File.Exists(file_path))
                {
                    data = new List<T>();
                }
                else
                {
                    data = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(file_path));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                return null;
            }

            return data;
        }

        public bool Save(List<T> configurations)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(file_path);
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(file_path, JsonConvert.SerializeObject(configurations, Formatting.Indented));

                data = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(file_path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configurations: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
