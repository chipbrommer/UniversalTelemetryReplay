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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Check if the input is numeric
            if (!int.TryParse(e.Text, out _))
            {
                // Prevent non-numeric input
                e.Handled = true;
            }
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Limit the length of the input to 5 numbers
            if (textBox.Text.Length > 5)
            {
                textBox.Text = textBox.Text[..5];
                // Set the caret position to the end
                textBox.CaretIndex = 5;
            }
        }

        private void UpdateAddButton()
        {
            // If we are at maximum items, disable button. 
            if (logItems.Count >= MainWindow.settings.MaxReplays)
            {
                AddLogItemButton.IsEnabled = false;
                AddLogItemButton.Content = "Disabled - Max Items Reached";
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
                openFileDialog.Filter = "Binary Files (*.bin)|*.bin|Digital Files (*.dig)|*.dig";
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

        private void LogParse_Click(object sender, RoutedEventArgs e)
        {

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
