using System.Windows;
using ConveniencePos.Data;
using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using ConveniencePos.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConveniencePos
{
    /// <summary>
    /// アプリケーションのエントリポイント。DIコンテナの構築とメインウィンドウの起動を行う。
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// アプリケーション起動時の処理。DIコンテナを構築し、メインウィンドウを表示する。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("PosDatabase")
                ?? throw new InvalidOperationException(
                    "接続文字列 'PosDatabase' が appsettings.json に定義されていません。");

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.AddDbContextFactory<PosDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddSingleton<IBarcodeService, BarcodeService>();
            services.AddSingleton<ITransactionService, TransactionService>();

            services.AddSingleton<IReceiptService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new ReceiptService(
                    storeName: config["Receipt:StoreName"] ?? "Convenience POS Store",
                    registerNumber: config["Receipt:RegisterNumber"] ?? "レジ#01",
                    operatorName: config["Receipt:OperatorName"] ?? "谷本 レジ担当",
                    outputDirectory: config["Receipt:OutputDirectory"] ?? "Desktop",
                    width: int.TryParse(config["Receipt:Width"], out var w) ? w : 32,
                    logger: loggerFactory.CreateLogger<ReceiptService>());
            });

            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            using (var scope = _serviceProvider.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
                using var dbContext = factory.CreateDbContext();
                dbContext.Database.EnsureCreated();
            }

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }

        /// <summary>
        /// アプリケーション終了時の処理。DIコンテナを破棄する。
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
