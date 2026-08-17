using System.Windows;
using System.Windows.Input;

namespace ConveniencePos.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BarcodeTextBox.Focus();
        }

        private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as ViewModels.MainViewModel;
                vm?.AddItemCommand.Execute(null);
                BarcodeTextBox.Focus();
            }
        }
    }
}