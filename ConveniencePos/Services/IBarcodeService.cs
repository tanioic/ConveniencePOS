using ConveniencePos.Models;

namespace ConveniencePos.Services;

public interface IBarcodeService
{
    Task<Product?> LookupByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
