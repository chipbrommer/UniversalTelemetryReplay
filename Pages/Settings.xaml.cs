using System.Windows;
using System.Windows.Controls;
using UniversalTelemetryReplay.Objects;
using static UniversalTelemetryReplay.MainWindow;

namespace UniversalTelemetryReplay.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            PopulateReplayLimitComboBox();
            PopulateParseLimitComboBox();
        }

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

        private void PopulateReplayLimitComboBox()
        {
            // Get all enum values from the ParseLimit enum
            var replayLimit = Enum.GetValues(typeof(MainWindow.ReplayLimit));

            // Set the ItemsSource of the ComboBox to the enum values
            ReplayComboBox.ItemsSource = replayLimit;
        }

        public void SetReplayLimitSelection(MainWindow.ReplayLimit replayLimit)
        {
            ReplayComboBox.SelectedItem = replayLimit;
        }

        private void ReplayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReplayComboBox.SelectedItem != null)
            {
                // Cast the selected item to MainWindow.ParseLimit enum type
                if (Enum.TryParse(ReplayComboBox.SelectedItem.ToString(), out MainWindow.ReplayLimit selectedLimit))
                {
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

        private void PopulateParseLimitComboBox()
        {
            // Get all enum values from the ParseLimit enum
            var parseLimitValues = Enum.GetValues(typeof(MainWindow.ParseLimit));

            // Set the ItemsSource of the ComboBox to the enum values
            ParseComboBox.ItemsSource = parseLimitValues;
        }

        public void SetParseLimitSelection(MainWindow.ParseLimit parseLimit)
        {
            ParseComboBox.SelectedItem = parseLimit;
        }

        private void ParseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParseComboBox.SelectedItem != null)
            {
                // Cast the selected item to MainWindow.ParseLimit enum type
                if (Enum.TryParse(ParseComboBox.SelectedItem.ToString(), out MainWindow.ParseLimit selectedLimit))
                {
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

    }
}
