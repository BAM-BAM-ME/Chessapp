using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Chessapp.Core;

namespace Gui
{
    public partial class InsightsView : UserControl
    {
        public InsightsService Service { get; set; } = new InsightsService();

        public InsightsView()
        {
            InitializeComponent();
        }

        public async Task RefreshAsync()
        {
            var rows = await Service.GetLatestAsync(50);
            Grid.ItemsSource = rows;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAsync();
        }
    }
}
