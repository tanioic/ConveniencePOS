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
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<PosDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("PosDatabase")));

            services.AddScoped<IBarcodeService, BarcodeService>();

            services.AddSingleton<IReceiptService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ReceiptService(
                    storeName: config["Receipt:StoreName"] ?? "Convenience POS Store",
                    registerNumber: config["Receipt:RegisterNumber"] ?? "レジ#01",
                    operatorName: config["Receipt:OperatorName"] ?? "谷本 レジ担当",
                    outputDirectory: config["Receipt:OutputDirectory"] ?? "Desktop",
                    width: int.TryParse(config["Receipt:Width"], out var w) ? w : 32);
            });

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
