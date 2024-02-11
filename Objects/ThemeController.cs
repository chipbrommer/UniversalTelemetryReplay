using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Controller for changing themes</summary>
    public class ThemeController
    {
        public enum ThemeTypes
        {
            Dark,
            Light,
            Modern,
            Navy,
            Traditional,
            Cyber
        }

        public static ResourceDictionary ThemeDictionary
        {
            get { return Application.Current.Resources.MergedDictionaries[0]; }
            set { Application.Current.Resources.MergedDictionaries[0] = value; }
        }

        private static void ChangeTheme(Uri uri)
        {
            ThemeDictionary = new ResourceDictionary() { Source = uri };
        }

        public static void SetTheme(ThemeTypes theme)
        {
            string themeName = theme switch
            {
                ThemeTypes.Dark => "Dark",
                ThemeTypes.Light => "Light",
                ThemeTypes.Navy => "Navy",
                ThemeTypes.Traditional => "Traditional",
                ThemeTypes.Cyber => "Cyber",
                // Intentional fall through 
                _ => "Modern",
            };

            try
            {
                if (MainWindow.settingsFile != null && MainWindow.settingsFile.data != null)
                {
                    MainWindow.settingsFile.data.Theme = theme;
                    MainWindow.settingsFile.Save();
                }

                if (!string.IsNullOrEmpty(themeName))
                    ChangeTheme(new Uri($"Themes/{themeName}.xaml", UriKind.Relative));
            }
            catch { }
        }
    }

}
