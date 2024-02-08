using Newtonsoft.Json;
using System.IO;

namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Template cass for loading and saving a settings file</summary>
    /// <typeparam name="T">Template representing the settings class</typeparam>
    public class SettingsFile<T> where T : class, new()
    {
        public T? data;             // Settings file data, template
        private string file_path;   // File path to the settings file

        /// <summary>Default constructor</summary>
        /// <param name="file_path">Full path to the settings file</param>
        public SettingsFile(string file_path)
        {
            this.file_path = file_path;
        }

        /// <summary>Load method for the settings file</summary>
        /// <returns>True if successful, else false</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(file_path))
                {
                    data = new T();
                }
                else
                {
                    data = JsonConvert.DeserializeObject<T>(File.ReadAllText(file_path));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return Save();
        }

        /// <summary>Save method for the settings file</summary>
        /// <returns>True if successful, else false</returns>
        public bool Save()
        {
            try
            {
                if (file_path != null && file_path != string.Empty)
                {
                    string? directoryName = Path.GetDirectoryName(file_path);
                    if (!Directory.Exists(directoryName) && directoryName != null)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    File.WriteAllText(file_path, JsonConvert.SerializeObject(data, Formatting.Indented));
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }

}
