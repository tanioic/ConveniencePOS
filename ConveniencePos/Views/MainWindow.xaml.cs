using System.Windows;
using System.Windows.Input;
using ConveniencePos.ViewModels;

namespace ConveniencePos.Views
{
    /// <summary>
    /// メインウィンドウ。バーコード入力と会計操作を行うPOS画面。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ。コンポーネントを初期化し、バーコード入力欄にフォーカスを設定する。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            BarcodeTextBox.Focus();
        }

        /// <summary>
        /// バーコード入力欄のキー押下イベント。Enterキーで商品を追加する。
        /// </summary>
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

        /// <summary>
        /// ウィンドウクローズ時の処理。ViewModelのリソースを解放する。
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
