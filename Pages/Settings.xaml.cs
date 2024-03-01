using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>Interaction logic for Settings Page</summary>
    public partial class Settings : Page
    {
        /// <summary> Constructor </summary>
        public Settings()
        {
            InitializeComponent();
            PopulateReplayLimitComboBox();
            PopulateParseLimitComboBox();
            PopulateTimestampDeltaLimitComboBox();
        }

        /// <summary> Set the Theme selection for the UI</summary>
        /// <param name="value">Theme to be selected</param>
        public void SetThemeSelection(ThemeController.ThemeTypes value)
        {
            switch (value)
            {
                case ThemeController.ThemeTypes.Cyber:          CyberTheme.IsChecked = true;        break;
                case ThemeController.ThemeTypes.Dark:           DarkTheme.IsChecked = true;         break;
                case ThemeController.ThemeTypes.Light:          LightTheme.IsChecked = true;        break;
                case ThemeController.ThemeTypes.Modern:         ModernTheme.IsChecked = true;       break;
                case ThemeController.ThemeTypes.Navy:           NavyTheme.IsChecked = true;         break;
                case ThemeController.ThemeTypes.Traditional:    TraditionalTheme.IsChecked = true;  break;
            }
        }

        /// <summary>Change event for the radio buttons</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Theme_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true && radioButton.Tag != null)
            {
                string selectedTheme = radioButton.Tag.ToString();
                // Apply the selected theme based on the value of selectedTheme
                switch (selectedTheme)
                {
                    case "Cyber":       ThemeController.SetTheme(ThemeController.ThemeTypes.Cyber);         break;
                    case "Dark":        ThemeController.SetTheme(ThemeController.ThemeTypes.Dark);          break;
                    case "Light":       ThemeController.SetTheme(ThemeController.ThemeTypes.Light);         break;
                    case "Modern":      ThemeController.SetTheme(ThemeController.ThemeTypes.Modern);        break;
                    case "Navy":        ThemeController.SetTheme(ThemeController.ThemeTypes.Navy);          break;
                    case "Traditional": ThemeController.SetTheme(ThemeController.ThemeTypes.Traditional);   break;
                }
            }
        }

        /// <summary>Populate the ReplayLimit Combo box </summary>
        private void PopulateReplayLimitComboBox()
        {
            // Get all enum values from the ReplayLimit enum
            var replayLimitValues = Enum.GetValues(typeof(MainWindow.ReplayLimit));

            // Create a list to hold the descriptions
            var descriptions = new List<string>();

            // Iterate over each enum value and retrieve its description
            foreach (var value in replayLimitValues)
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                // If a description is found, add it to the list
                if (attributes.Length > 0)
                {
                    descriptions.Add(attributes[0].Description);
                }
                else
                {
                    // If no description is found, use the enum name as the description
                    descriptions.Add(value.ToString());
                }
            }

            // Set the ItemsSource of the ComboBox to the descriptions
            ReplayComboBox.ItemsSource = descriptions;
        }

        /// <summary>Set the ReplayLimit selection</summary>
        /// <param name="replayLimit">selected limit</param>
        public void SetReplayLimitSelection(MainWindow.ReplayLimit limit)
        {
            // Get the description corresponding to the enum value
            string description = GetEnumDescription(limit);

            // Set the selected item of the combo box to the description
            ReplayComboBox.SelectedItem = description;
        }

        /// <summary>Change tvent for the ReplayLimit combo box</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReplayComboBox.SelectedItem != null)
            {
                // Get the selected description from the combo box
                string selectedDescription = ReplayComboBox.SelectedItem.ToString();

                if (selectedDescription != null)
                {
                    // Find the corresponding enum value based on the description
                    MainWindow.ReplayLimit selectedLimit = GetEnumValueFromDescription<MainWindow.ReplayLimit>(selectedDescription);

                    if (MainWindow.settingsFile != null &&
                        MainWindow.settingsFile.data != null &&
                        MainWindow.settingsFile.data.ReplayLimit != selectedLimit
                        )
                    {
                        MainWindow.settingsFile.data.ReplayLimit = selectedLimit;
                        MainWindow.settingsFile.Save();
                    }
                }
            }
        }

        /// <summary>Populate the ParseLimit combo box</summary>
        private void PopulateParseLimitComboBox()
        {
            // Get all enum values from the ParseLimit enum
            var parseLimitValues = Enum.GetValues(typeof(MainWindow.ParseLimit));

            // Create a list to hold the descriptions
            var descriptions = new List<string>();

            // Iterate over each enum value and retrieve its description
            foreach (var value in parseLimitValues)
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                // If a description is found, add it to the list
                if (attributes.Length > 0)
                {
                    descriptions.Add(attributes[0].Description);
                }
                else
                {
                    // If no description is found, use the enum name as the description
                    descriptions.Add(value.ToString());
                }
            }

            // Set the ItemsSource of the ComboBox to the descriptions
            ParseComboBox.ItemsSource = descriptions;
        }

        /// <summary> Set the ParseLimit selection</summary>
        /// <param name="parseLimit">selected limit</param>
        public void SetParseLimitSelection(MainWindow.ParseLimit limit)
        {
            // Get the description corresponding to the enum value
            string description = GetEnumDescription(limit);

            // Set the selected item of the combo box to the description
            ParseComboBox.SelectedItem = description;
        }

        /// <summary>Change event for the Parse limit combo box</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParseComboBox.SelectedItem != null)
            {
                // Get the selected description from the combo box
                string selectedDescription = ParseComboBox.SelectedItem.ToString();

                if(selectedDescription != null)
                {
                    // Find the corresponding enum value based on the description
                    MainWindow.ParseLimit selectedLimit = GetEnumValueFromDescription<MainWindow.ParseLimit>(selectedDescription);

                    if (MainWindow.settingsFile != null &&
                        MainWindow.settingsFile.data != null &&
                        MainWindow.settingsFile.data.ParseLimit != selectedLimit
                        )
                    {
                        MainWindow.settingsFile.data.ParseLimit = selectedLimit;
                        MainWindow.settingsFile.Save();
                    }
                }
            }
        }

        /// <summary>Populate the TimestampDeltaLimit combo box</summary>
        private void PopulateTimestampDeltaLimitComboBox()
        {
            // Get all enum values from the TimestampDeltaLimit enum
            var deltaLimitValues = Enum.GetValues(typeof(MainWindow.TimestampDeltaLimit));

            // Create a list to hold the descriptions
            var descriptions = new List<string>();

            // Iterate over each enum value and retrieve its description
            foreach (var value in deltaLimitValues)
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                // If a description is found, add it to the list
                if (attributes.Length > 0)
                {
                    descriptions.Add(attributes[0].Description);
                }
                else
                {
                    // If no description is found, use the enum name as the description
                    descriptions.Add(value.ToString());
                }
            }

            // Set the ItemsSource of the ComboBox to the descriptions
            TimestampComboBox.ItemsSource = descriptions;
        }

        /// <summary> Set the TimestampDeltaLimit selection</summary>
        /// <param name="limit">selected limit</param>
        public void SetTimestampDeltaLimitSelection(MainWindow.TimestampDeltaLimit limit)
        {
            // Get the description corresponding to the enum value
            string description = GetEnumDescription(limit);

            // Set the selected item of the combo box to the description
            TimestampComboBox.SelectedItem = description;
        }

        /// <summary>Change event for the Timestamp combo box</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimestampComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimestampComboBox.SelectedItem != null)
            {
                // Get the selected description from the combo box
                string selectedDescription = TimestampComboBox.SelectedItem.ToString();

                // Find the corresponding enum value based on the description
                MainWindow.TimestampDeltaLimit selectedLimit = GetEnumValueFromDescription<MainWindow.TimestampDeltaLimit>(selectedDescription);

                // Update settings if necessary
                if (MainWindow.settingsFile != null &&
                    MainWindow.settingsFile.data != null &&
                    MainWindow.settingsFile.data.TimestampDeltaLimit != selectedLimit)
                {
                    MainWindow.settingsFile.data.TimestampDeltaLimit = selectedLimit;
                    MainWindow.settingsFile.Save();
                }
            }
        }

        // Helper method to get the description of an enum value
        private static string GetEnumDescription(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        // Helper method to get the enum value from its description
        private static T GetEnumValueFromDescription<T>(string description)
        {
            Array values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                if (GetEnumDescription((Enum)value) == description)
                {
                    return (T)value;
                }
            }
            throw new ArgumentException($"Enum value with description '{description}' not found.");
        }

    }

}
