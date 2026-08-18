using System.Windows;
using ConveniencePos.Data;
using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using ConveniencePos.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConveniencePos;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            MessageBox.Show("接続文字列が設定されていません。appsettings.json を確認してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        services.AddDbContextFactory<PosDbContext>(options => options.UseSqlServer(connectionString));

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
        services.AddSingleton(configuration);
        services.AddSingleton<IConfiguration>(configuration);

        ServiceProvider = services.BuildServiceProvider();

        try
        {
            using var scope = ServiceProvider.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"データベースの初期化に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ServiceProvider?.Dispose();
        base.OnExit(e);
    }
}
