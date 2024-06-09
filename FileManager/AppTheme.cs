using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager
{
     class AppTheme
    {
        public static void ChangeTheme(string name)
        {
            if (name == "Default")
            {
                name = "Light";
            }
            Uri uri = new Uri($"Themes/{name}.xaml", UriKind.Relative);
            ResourceDictionary theme = new ResourceDictionary() { Source = uri };

            App.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(theme);

        }
    }
}
