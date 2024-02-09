using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>
    /// Interaction logic for Configure.xaml
    /// </summary>
    public partial class Configure : Page
    {
        MainWindow mainWindow;

        public Configure(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;   
        }

        private void AddConfiguration_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve values from the input fields
            string name = NameTextBox.Text;
            List<byte> syncBytes = ParseBytes(SyncBytesTextBox.Text);
            uint messageSize = ParseUint(MessageSizeTextBox.Text);
            uint timestampLocation = ParseUint(TimestampLocationTextBox.Text);
            List<byte> endBytes = ParseBytes(EndBytesTextBox.Text);

            // Create a new MessageConfiguration object
            MessageConfiguration newConfiguration = new MessageConfiguration
            {
                Name = name,
                SyncBytes = syncBytes,
                MessageSize = messageSize,
                TimestampLocation = timestampLocation,
                EndBytes = endBytes
            };

            // Add the new configuration to the ConfigurationManager
            if (MainWindow.configManager != null)
            {
                MainWindow.configManager.AddConfiguration(newConfiguration);
                // Save the configurations to file
                MainWindow.configManager.Save();

                // Update the DataGrid
                UpdateConfigurationsDataGrid();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the configuration associated with the clicked row
            Button deleteButton = sender as Button;
            MessageConfiguration configuration = deleteButton.DataContext as MessageConfiguration;

            // Get the index of the configuration
            int index = MainWindow.configManager.GetData().IndexOf(configuration);

            // Remove the configuration from the ConfigurationManager
            if (MainWindow.configManager != null && index != -1)
            {
                MainWindow.configManager.RemoveConfiguration(index);
                // Save the configurations to file
                MainWindow.configManager.Save();

                // Update the DataGrid
                UpdateConfigurationsDataGrid();
            }
        }

        private List<byte> ParseBytes(string text)
        {
            // Parse the text input as a list of bytes (e.g., "1 2 3 4" -> [1, 2, 3, 4])
            List<byte> bytes = [];
            string[] byteStrings = text.Split(' ');
            foreach (string byteString in byteStrings)
            {
                if (byte.TryParse(byteString, out byte parsedByte))
                {
                    bytes.Add(parsedByte);
                }
            }
            return bytes;
        }

        private uint ParseUint(string text)
        {
            // Parse the text input as a uint
            if (uint.TryParse(text, out uint parsedUint))
            {
                return parsedUint;
            }
            return 0; // Return 0 if parsing fails
        }

        public void UpdateConfigurationsDataGrid()
        {
            // Update the DataGrid with the latest configurations
            if (MainWindow.configManager != null)
            {
                // Get the configurations data from ConfigurationManager
                List<MessageConfiguration> configurations = MainWindow.configManager.GetData();

                // Add row index number to each configuration
                for (int i = 0; i < configurations.Count; i++)
                {
                    configurations[i].RowIndex = i;
                }

                // Set the ItemsSource of DataGrid to the configurations data
                ConfigurationsDataGrid.ItemsSource = configurations;
            }
        }

        private void DeleteButton_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show the delete button when the mouse enters the cell
            Button deleteButton = sender as Button;
            deleteButton.Visibility = Visibility.Visible;
        }

        private void DeleteButton_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide the delete button when the mouse leaves the cell
            Button deleteButton = sender as Button;
            deleteButton.Visibility = Visibility.Collapsed;
        }
    }
}
