using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConveniencePos.Services;

public class ReceiptService : IReceiptService
{
    private readonly string _storeName;
    private readonly string _registerNumber;
    private readonly string _operatorName;
    private readonly string _outputDirectory;
    private readonly int _width;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        string storeName = "Convenience POS Store",
        string registerNumber = "レジ#01",
        string operatorName = "谷本 レジ担当",
        string outputDirectory = "Desktop",
        int width = 32,
        ILogger<ReceiptService>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(registerNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        _storeName = storeName;
        _registerNumber = registerNumber;
        _operatorName = operatorName;
        _outputDirectory = outputDirectory;
        _width = width;
        _logger = logger ?? NullLogger<ReceiptService>.Instance;
    }

    public string GenerateReceipt(ReceiptContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sb = new StringBuilder();
        sb.AppendLine(new string('=', _width));
        sb.AppendLine(CenterText(_storeName, _width));
        sb.AppendLine($"{_registerNumber}  担当: {_operatorName}");
        sb.AppendLine();
        sb.AppendLine($"取引番号: TRX-{context.TransactionId}");
        sb.AppendLine(context.TransactionTime.ToString("yyyy/MM/dd HH:mm"));
        sb.AppendLine();
        foreach (var item in context.Items)
        {
            sb.AppendLine($"{item.Name} {item.Quantity}  \u00a5{item.LineTotalWithTax:N0} {item.TaxRate}%");
        }
        sb.AppendLine(AmountLine("税抜合計", $"\u00a5{context.Subtotal:N0}"));
        sb.AppendLine($"8% 対象: \u00a5{context.TaxableAmount8:N0} 消費税: \u00a5{context.TaxAmount8:N0}");
        sb.AppendLine($"10%対象: \u00a5{context.TaxableAmount10:N0} 消費税: \u00a5{context.TaxAmount10:N0}");
        sb.AppendLine(AmountLine("消費税合計", $"\u00a5{context.TaxAmount:N0}"));
        sb.AppendLine();
        sb.AppendLine(AmountLine("税込合計", $"\u00a5{context.TotalAmount:N0}"));
        sb.AppendLine(AmountLine("[現金] お預かり", $"\u00a5{context.ReceivedAmount:N0}"));
        sb.AppendLine(AmountLine("お釣り", $"\u00a5{context.Change:N0}"));
        sb.AppendLine("ありがとうお越し下さいました");
        return sb.ToString();
    }

    public async Task SaveReceiptAsync(int transactionId, string receiptContent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(receiptContent);

        var basePath = ResolveOutputDirectory();
        Directory.CreateDirectory(basePath);

        var filePath = Path.Combine(basePath, $"receipt_{transactionId}.txt");
        await File.WriteAllTextAsync(filePath, receiptContent, cancellationToken);
        _logger.LogDebug("レシートを保存しました: {FilePath}", filePath);
    }

    private string ResolveOutputDirectory() => _outputDirectory switch
    {
        "Desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        _ => _outputDirectory
    };

    private int DisplayWidth(string s)
    {
        int width = 0;
        foreach (var c in s)
        {
            if (c >= 0x3000 && c < 0xA000) width += 2;
            else if (c >= 0xAC00 && c < 0xD800) width += 2;
            else if (c >= 0xF900 && c < 0xFB00) width += 2;
            else if (c >= 0xFF01 && c < 0xFF5F) width += 2;
            else width += 1;
        }
        return width;
    }

    private string CenterText(string text, int totalWidth)
    {
        int pad = Math.Max(0, (totalWidth - DisplayWidth(text)) / 2);
        return new string(' ', pad) + text;
    }

    private string AmountLine(string label, string amount)
    {
        int spaces = _width - DisplayWidth(label) - DisplayWidth(amount);
        return label + new string(' ', Math.Max(1, spaces)) + amount;
    }
}
