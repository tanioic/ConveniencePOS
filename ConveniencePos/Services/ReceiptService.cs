using System.Text;

namespace ConveniencePos.Services;

public class ReceiptService : IReceiptService
{
    private readonly string _storeName;
    private readonly string _registerNumber;
    private readonly string _operatorName;
    private readonly string _outputDirectory;
    private readonly int _width;

    public ReceiptService(
        string storeName = "Convenience POS Store",
        string registerNumber = "レジ#01",
        string operatorName = "谷本 レジ担当",
        string outputDirectory = "Desktop",
        int width = 32)
    {
        _storeName = storeName;
        _registerNumber = registerNumber;
        _operatorName = operatorName;
        _outputDirectory = outputDirectory;
        _width = width;
    }

    public string GenerateReceipt(
        int transactionId,
        DateTime transactionTime,
        IReadOnlyList<ReceiptItem> items,
        decimal subtotal,
        decimal taxableAmount8,
        decimal taxableAmount10,
        decimal taxAmount8,
        decimal taxAmount10,
        decimal taxAmount,
        decimal totalAmount,
        decimal receivedAmount,
        decimal change)
    {
        var sb = new StringBuilder();
        sb.AppendLine(new string('=', _width));
        sb.AppendLine(CenterText(_storeName, _width));
        sb.AppendLine($"{_registerNumber}  担当: {_operatorName}");
        sb.AppendLine();
        sb.AppendLine($"取引番号: TRX-{transactionId}");
        sb.AppendLine(transactionTime.ToString("yyyy/MM/dd HH:mm"));
        sb.AppendLine();
        foreach (var item in items)
        {
            sb.AppendLine($"{item.Name} {item.Quantity}  \u00a5{item.LineTotalWithTax:N0} {item.TaxRate}%");
        }
        sb.AppendLine(AmountLine("税抜合計", $"\u00a5{subtotal:N0}"));
        sb.AppendLine($"8% 対象: \u00a5{taxableAmount8:N0} 消費税: \u00a5{taxAmount8:N0}");
        sb.AppendLine($"10%対象: \u00a5{taxableAmount10:N0} 消費税: \u00a5{taxAmount10:N0}");
        sb.AppendLine(AmountLine("消費税合計", $"\u00a5{taxAmount:N0}"));
        sb.AppendLine();
        sb.AppendLine(AmountLine("税込合計", $"\u00a5{totalAmount:N0}"));
        sb.AppendLine(AmountLine("[現金] お預かり", $"\u00a5{receivedAmount:N0}"));
        sb.AppendLine(AmountLine("お釣り", $"\u00a5{change:N0}"));
        sb.AppendLine("ありがとうお越し下さいました");
        return sb.ToString();
    }

    public void SaveReceipt(int transactionId, string receiptContent)
    {
        var basePath = _outputDirectory switch
        {
            "Desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            _ => _outputDirectory
        };
        var filePath = System.IO.Path.Combine(basePath, $"receipt_{transactionId}.txt");
        System.IO.File.WriteAllText(filePath, receiptContent);
    }

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
