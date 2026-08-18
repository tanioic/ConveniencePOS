using System.Windows;
using System.Windows.Input;
using ConveniencePos.ViewModels;

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
                if (DataContext is MainViewModel vm)
                {
                    vm.AddItemCommand.Execute(null);
                }
                BarcodeTextBox.Focus();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
