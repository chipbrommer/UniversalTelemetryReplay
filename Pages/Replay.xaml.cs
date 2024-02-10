﻿using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>
    /// Interaction logic for Replay.xaml
    /// </summary>
    public partial class Replay : Page
    {
        public readonly ObservableCollection<LogItem> logItems;
        readonly MainWindow mw;

        public Replay(MainWindow main)
        {
            InitializeComponent();
            logItems = [];
            mw = main;

            // Set DataContext to logItems for binding
            DataContext = logItems;
        }

        private void AddLogItem_Click(object sender, RoutedEventArgs e)
        {
            // Preventive check
            if (MainWindow.settingsFile == null || MainWindow.settingsFile.data == null) return;
            if (logItems.Count > MainWindow.settingsFile.data.MaxReplays) return;

            // Add a new log item to the logItems collection
            logItems.Add(new LogItem()
            {
                Id = LogItem.NEXT_ID++,
                Configuration = "Unknown",
                ConfigBG = (Brush)Application.Current.Resources["PrimaryGrayColor"],
                FilePath = "Not Selected",
                Status = "Not Parsed",
                StatusBG = (Brush)Application.Current.Resources["PrimaryGrayColor"],
                IpAddress = "127.0.0.1",
                Port = 5700,
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
            if (sender is not TextBox textBox) return;

            // Limit the length of the input to 5 numbers
            if (textBox.Text.Length > 5)
            {
                textBox.Text = textBox.Text[..5];
                // Set the caret position to the end
                textBox.CaretIndex = 5;
            }

            // Find the parent of the TextBox
            if (textBox.Parent is StackPanel parent)
            {
                // Find the "Update" button by its name
                if (parent.FindName("UpdateButton") is not Button updateButton) return;

                // Enable/disable the "Update" button based on the Port TextBox's text
                // hide it if the port is empty to prevent the update. 
                if (!string.IsNullOrEmpty(textBox.Text))
                    updateButton.Visibility = Visibility.Visible;
                else
                    updateButton.Visibility = Visibility.Hidden;
            }

        }

        private void UpdateAddButton()
        {
            if (MainWindow.settingsFile == null || MainWindow.settingsFile.data == null) return;

            // If we are at maximum items, disable button. 
            if (logItems.Count >= MainWindow.settingsFile.data.MaxReplays)
            {
                AddLogItemButton.IsEnabled = false;
                AddLogItemButton.Content = "Max Replay Items Reached";
                TotalItemsTextBlock.Foreground = (Brush)Application.Current.Resources["PrimaryRedColor"];
            }
            else
            {
                AddLogItemButton.IsEnabled = true;
                AddLogItemButton.Content = "Add Log to Replay";
                TotalItemsTextBlock.Foreground = (Brush)Application.Current.Resources["TextSecondaryColor"];
            }

            if (logItems.Count > 0)
                mw.UpdateControls(true);
            else
                mw.UpdateControls(false);
        }

        private void LogBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button? browseButton = sender as Button;

            // Get the data context of the button, which is the item in the list
            if (browseButton.DataContext is LogItem logItem)
            {
                // Create OpenFileDialog
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Digital Files (*.dig)|*.dig|Binary Files (*.bin)|*.bin",
                    FilterIndex = 1,
                    Multiselect = false
                };

                // Show OpenFileDialog
                if (openFileDialog.ShowDialog() == true)
                {
                    // Get the selected file path
                    string selectedFilePath = openFileDialog.FileName;

                    // Find the index of the logItem in the ObservableCollection and update it
                    int index = logItems.IndexOf(logItem);
                    logItems[index].FilePath = selectedFilePath;
                    logItems[index].pathSelected = true;
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
            if (button.Parent is StackPanel parent)
            {
                // Find the "Update" button by its name and hide it
                Button updateButton = parent.FindName("UpdateButton") as Button;
                updateButton.Visibility = Visibility.Hidden;
            }
        }

        private void LogRemove_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button? removeButton = sender as Button;

            // Get the data context of the button, which is the item in the list
            if (removeButton.DataContext != null)
            {
                LogItem itemToRemove = removeButton.DataContext as LogItem;

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
