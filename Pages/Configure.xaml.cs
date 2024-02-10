using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>
    /// Interaction logic for Configure.xaml
    /// </summary>
    public partial class Configure : Page
    {
        public Configure()
        {
            InitializeComponent(); 
        }

        private void AddUpdateConfiguration_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve value from the Name field (which is required)
            string name = NameTextBox.Text.Trim();

            // Check if the Name field is empty
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name for the configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Stop further execution
            }

            // Retrieve values from the optional input fields
            byte sync1 = ParseByte(SyncByte1TextBox.Text);
            byte sync2 = ParseByte(SyncByte2TextBox.Text);
            byte sync3 = ParseByte(SyncByte3TextBox.Text);
            byte sync4 = ParseByte(SyncByte4TextBox.Text);
            uint messageSize = ParseUint(MessageSizeTextBox.Text);
            uint timestampSize = ParseUint(TimestampSizeTextBox.Text);
            uint timestampOffset = ParseUint(TimestampOffsetTextBox.Text);
            double timestampScaling = EvaluateExpression(TimestampScalingTextBox.Text);
            byte end1 = ParseByte(EndByte1TextBox.Text);
            byte end2 = ParseByte(EndByte2TextBox.Text);
            int index = ParseInt(IndexTextBox.Text);

            // Create a new MessageConfiguration object
            MessageConfiguration newConfiguration = new()
            {
                Name = name,
                SyncByte1 = sync1,
                SyncByte2 = sync2,
                SyncByte3 = sync3,
                SyncByte4 = sync4,
                MessageSize = messageSize,
                TimestampSize = timestampSize,
                TimestampByteOffset = timestampOffset,
                TimestampScaling = timestampScaling,
                EndByte1 = end1,
                EndByte2 = end2,
                RowIndex = index,
            };

            if (MainWindow.configManager != null)
            {
                if (index < 0) MainWindow.configManager.AddConfiguration(newConfiguration);
                else MainWindow.configManager.UpdateConfiguration(index, newConfiguration);

                // Save the configurations to file
                MainWindow.configManager.Save();

                // Update the DataGrid
                UpdateConfigurationsDataGrid();

                // Reset Fields
                ResetFields_Click(null, null);
            }
        }

        private void ResetFields_Click(object? sender, RoutedEventArgs? e)
        {
            NameTextBox.Text = "";
            SyncByte1TextBox.Text = "";
            SyncByte2TextBox.Text = "";
            SyncByte3TextBox.Text = "";
            SyncByte4TextBox.Text = "";
            MessageSizeTextBox.Text = "";
            TimestampSizeTextBox.Text = "";
            TimestampOffsetTextBox.Text = "";
            TimestampScalingTextBox.Text = "";
            EndByte1TextBox.Text = "";
            EndByte2TextBox.Text = "";
            IndexTextBox.Text = "-1";

            // Unselect the selected item in the DataGrid
            ConfigurationsDataGrid.SelectedItem = null;
        }

        private void DeleteConfiguration_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the DataGrid

            // Remove the selected configuration
            if (MainWindow.configManager != null && ConfigurationsDataGrid.SelectedItem is MessageConfiguration selectedConfiguration)
            {
                MainWindow.configManager.RemoveConfiguration(selectedConfiguration.RowIndex);
                // Save the configurations to file
                MainWindow.configManager.Save();

                // Update the DataGrid
                UpdateConfigurationsDataGrid();

                // Reset Fields
                ResetFields_Click(null, null);
            }
        }

        private void DeleteAllConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.configManager == null)
            {
                return;
            }

            // Display confirmation message
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all configurations?", "Confirm Deletion", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            // Check user's response
            if (result == MessageBoxResult.OK)
            {

                // Clear all configurations
                MainWindow.configManager.GetData().Clear();
                // Save the configurations to file
                MainWindow.configManager.Save();

                // Update the DataGrid
                UpdateConfigurationsDataGrid();

                // Reset Fields
                ResetFields_Click(null, null);
            }
        }


        public static byte ParseByte(string byteValue)
        {
            // Check if the text is empty or null - return 0
            if (string.IsNullOrEmpty(byteValue))
            {
                return 0;
            }

            // Check if the string starts with "0x"
            if (byteValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                // Remove "0x" prefix and parse the remaining hexadecimal value
                return byte.Parse(byteValue[2..], NumberStyles.HexNumber);
            }
            else
            {
                // Parse the byte value directly
                return byte.Parse(byteValue);
            }
        }

        private static int ParseInt(string text)
        {
            // Check if the text is empty or null - return 0
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            // Parse the text input as a int
            if (int.TryParse(text, out int parsedInt))
            {
                return parsedInt;
            }
            return 0; // Return 0 if parsing fails
        }

        private static uint ParseUint(string text)
        {
            // Check if the text is empty or null - return 0
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            // Parse the text input as a uint
            if (uint.TryParse(text, out uint parsedUint))
            {
                return parsedUint;
            }
            return 0; // Return 0 if parsing fails
        }

        private static double ParseDouble(string text)
        {
            // Check if the text is empty or null - return 0.0
            if (string.IsNullOrEmpty(text))
            {
                return 0.0;
            }

            // Parse the text input as a int
            if (double.TryParse(text, out double parsed))
            {
                return parsed;
            }
            return 0.0; // Return 0.0 if parsing fails
        }

        private static double EvaluateExpression(string expression)
        {
            if (expression.Contains("^"))
            {
                // If the expression contains '^', use Math.Pow
                string[] parts = expression.Split('^');
                double baseValue = double.Parse(parts[0]);
                double exponent = double.Parse(parts[1]);
                return Math.Pow(baseValue, exponent);
            }
            else
            {
                // If the expression doesn't contain '^', use DataTable.Compute
                // to get the expression value. 
                DataTable dt = new DataTable();
                object result = dt.Compute(expression, "");
                return Convert.ToDouble(result);
            }
        }

        public void UpdateConfigurationsDataGrid()
        {
            // Update the DataGrid with the latest configurations
            if (MainWindow.configManager != null)
            {
                // Get the configurations data from ConfigurationManager
                ObservableCollection<MessageConfiguration> configurations = new(MainWindow.configManager.GetData());

                // Add row index number to each configuration
                for (int i = 0; i < configurations.Count; i++)
                {
                    configurations[i].RowIndex = i;
                }

                // Set the ItemsSource of DataGrid to the configurations data
                ConfigurationsDataGrid.ItemsSource = configurations;
            }
        }

        private void ConfigurationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Populate the text boxes with the selected configuration's data
            if (ConfigurationsDataGrid.SelectedItem is MessageConfiguration selectedConfiguration)
            {
                NameTextBox.Text = selectedConfiguration.Name;
                SyncByte1TextBox.Text = selectedConfiguration.SyncByte1.ToString();
                SyncByte2TextBox.Text = selectedConfiguration.SyncByte2.ToString();
                SyncByte3TextBox.Text = selectedConfiguration.SyncByte3.ToString();
                SyncByte4TextBox.Text = selectedConfiguration.SyncByte4.ToString();
                MessageSizeTextBox.Text = selectedConfiguration.MessageSize.ToString();
                TimestampSizeTextBox.Text = selectedConfiguration.TimestampSize.ToString();
                TimestampOffsetTextBox.Text = selectedConfiguration.TimestampByteOffset.ToString();
                TimestampScalingTextBox.Text = selectedConfiguration.TimestampScaling.ToString();
                EndByte1TextBox.Text = selectedConfiguration.EndByte1.ToString();
                EndByte2TextBox.Text = selectedConfiguration.EndByte2.ToString();
                IndexTextBox.Text = selectedConfiguration.RowIndex.ToString();
            }
        }

        private void LoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Loading a configuration file will overwrite any current configurations. Do you want to continue?", "Confirm Load", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() == true && MainWindow.configManager != null)
                {
                    // Load configurations from the selected file
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        if (MainWindow.configManager.LoadConfigFromFile(filePath))
                        {
                            UpdateConfigurationsDataGrid();
                            MessageBox.Show("Configurations loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                            MessageBox.Show("Configurations failed to load.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading configurations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExportConfiguration_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Save configurations to the selected file
                try
                {
                    if (MainWindow.configManager != null)
                    {
                        ObservableCollection<MessageConfiguration> configurations = MainWindow.configManager.GetData();
                        string json = JsonConvert.SerializeObject(configurations, Formatting.Indented);
                        File.WriteAllText(filePath, json);
                        MessageBox.Show("Configurations exported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting configurations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
