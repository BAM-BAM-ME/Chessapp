using System.IO;
using System.Windows;
using Microsoft.Win32;
using Chessapp.Core;

namespace Gui
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var cfg = ConfigService.LoadAppSettings();
            TxtEnginePath.Text = cfg.EnginePath;
            TxtDepth.Text = cfg.Depth.ToString();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Engine (*.exe)|*.exe" };
            if (ofd.ShowDialog() == true)
            {
                TxtEnginePath.Text = ofd.FileName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(TxtEnginePath.Text))
            {
                MessageBox.Show("Engine file not found.");
                return;
            }
            if (!int.TryParse(TxtDepth.Text, out var depth) || depth <= 0)
            {
                MessageBox.Show("Depth must be a positive number.");
                return;
            }
            var cfg = ConfigService.LoadAppSettings();
            cfg.EnginePath = TxtEnginePath.Text;
            cfg.Depth = depth;
            ConfigService.SaveAppSettings(cfg);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
