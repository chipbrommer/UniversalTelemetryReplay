using System.Windows;
using System.Windows.Controls;
using UniversalTelemetryReplay.Objects;

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
        }

        public void SetThemeSelection(ThemeController.ThemeTypes value)
        {
            switch (value)
            {
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
                    case "Dark":        ThemeController.SetTheme(ThemeController.ThemeTypes.Dark);          break;
                    case "Light":       ThemeController.SetTheme(ThemeController.ThemeTypes.Light);         break;
                    case "Modern":      ThemeController.SetTheme(ThemeController.ThemeTypes.Modern);        break;
                    case "Navy":        ThemeController.SetTheme(ThemeController.ThemeTypes.Navy);          break;
                    case "Traditional": ThemeController.SetTheme(ThemeController.ThemeTypes.Traditional);   break;
                }
            }
        }
    }
}
