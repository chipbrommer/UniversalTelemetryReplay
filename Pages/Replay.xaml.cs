using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>
    /// Interaction logic for Replay.xaml
    /// </summary>
    public partial class Replay : Page
    {
        private readonly ObservableCollection<LogItem> logItems;

        public Replay()
        {
            InitializeComponent();
            logItems = [];

            // Set DataContext to logItems for binding
            DataContext = logItems;
        }

        private void AddLogItem_Click(object sender, RoutedEventArgs e)
        {
            // Preventive check
            if (logItems.Count > MainWindow.settings.MaxReplays) return;

            // Add a new log item to the logItems collection
            logItems.Add(new LogItem()
            {
                Id = LogItem.NEXT_ID++,
                Configuration = "Unknown",
                FilePath = "Not Selected",
                Status = "Not Parsed",
                IpAddress = "127.0.0.1",
                Port = 5000,
            });

            UpdateAddButton();
        }

        private void PortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if the input is numeric
            if (!int.TryParse(e.Text, out _))
            {
                // Prevent non-numeric input
                e.Handled = true;
            }
        }


        private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Limit the length of the input to 5 numbers
            if (textBox.Text.Length > 5)
            {
                textBox.Text = textBox.Text[..5];
                // Set the caret position to the end
                textBox.CaretIndex = 5;
            }

            // Find the parent of the TextBox
            StackPanel parent = textBox.Parent as StackPanel;
            if (parent != null)
            {
                // Find the "Update" button by its name
                Button updateButton = parent.FindName("UpdateButton") as Button;

                // Enable/disable the "Update" button based on the Port TextBox's text
                // hide it if the port is empty to prevent the update. 
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    updateButton.Visibility = Visibility.Visible;
                }
                else
                {
                    updateButton.Visibility = Visibility.Hidden;
                }
            }

        }

        private void UpdateAddButton()
        {
            // If we are at maximum items, disable button. 
            if (logItems.Count >= MainWindow.settings.MaxReplays)
            {
                AddLogItemButton.IsEnabled = false;
                AddLogItemButton.Content = "Max Replay Items Reached";
                TotalItemsTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryRedColor"];
            }
            else
            {
                AddLogItemButton.IsEnabled = true;
                AddLogItemButton.Content = "Add Log Item";
                TotalItemsTextBlock.Foreground = (Brush)Application.Current.Resources["TextSecondaryColor"];
            }
        }

        private void LogBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button browseButton = sender as Button;

            // Get the data context of the button, which is the item in the list
            if (browseButton.DataContext is LogItem logItem)
            {
                // Create OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Digital Files (*.dig)|*.dig|Binary Files (*.bin)|*.bin";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;

                // Show OpenFileDialog
                if (openFileDialog.ShowDialog() == true)
                {
                    // Get the selected file path
                    string selectedFilePath = openFileDialog.FileName;

                    // Find the index of the logItem in the ObservableCollection and update it
                    int index = logItems.IndexOf(logItem);
                    logItems[index].FilePath = selectedFilePath;
                }
            }
        }

        private void LogUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button button = sender as Button;

            // Get the data context of the button, which is the item in the list
            if (button.DataContext is LogItem logItem)
            {
                // Find the index of the logItem in the ObservableCollection and update it
                int index = logItems.IndexOf(logItem);
                logItems[index].Port = logItem.Port;
                logItems[index].IpAddress = logItem.IpAddress;
            }

            // Find the parent of the Button
            StackPanel parent = button.Parent as StackPanel;
            if (parent != null)
            {
                // Find the "Update" button by its name and hide it
                Button updateButton = parent.FindName("UpdateButton") as Button;
                updateButton.Visibility = Visibility.Hidden;
            }
        }

        private void LogRemove_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button removeButton = sender as Button;

            // Get the data context of the button, which is the item in the list
            if (removeButton.DataContext != null)
            {
                var itemToRemove = removeButton.DataContext as LogItem;

                // Assuming your ItemsSource is an ObservableCollection, you can remove the item
                if (logItems is ObservableCollection<LogItem> observableCollection)
                {
                    observableCollection.Remove(itemToRemove);
                }

                UpdateAddButton();
            }
        }
    }
}
