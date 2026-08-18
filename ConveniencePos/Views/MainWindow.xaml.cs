using System.Windows;
using System.Windows.Input;
using ConveniencePos.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ConveniencePos.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BarcodeTextBox.Focus();
    }

    private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel vm)
        {
            vm.AddItemCommand.Execute(null);
            BarcodeTextBox.Focus();
        }
    }
}
