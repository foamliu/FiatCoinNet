using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FiatCoinNet.WalletGui
{
    public enum LanguagePreference
    {
        English = 0,

        中文 = 1,

    }

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            BindLanguageSettings();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BindLanguageSettings()
        {
            comboBoxLanguage.ItemsSource = Enum.GetNames(typeof(LanguagePreference)).ToList();
            comboBoxLanguage.SelectedIndex = 1;
        }

        private void comboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.comboBoxLanguage.SelectedIndex == 0)
            {
                LoadLanguageFile("pack://application:,,,/lang/en-us.xaml");
            }
            else
            {
                LoadLanguageFile("pack://application:,,,/lang/zh-cn.xaml");
            }
        }

        private void LoadLanguageFile(string languageFileName)
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(languageFileName, UriKind.RelativeOrAbsolute)
            };
        }
    }
}
