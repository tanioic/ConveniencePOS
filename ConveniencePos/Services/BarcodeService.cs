using ConveniencePos.Data;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;

namespace ConveniencePos.Services;

/// <summary>
/// バーコードJANコードによる商品検索の実装。
/// PosDbContext を使用してDBから商品を検索する。
/// </summary>
public class BarcodeService : IBarcodeService
{
    private readonly PosDbContext _dbContext;

    public BarcodeService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<Product?> LookupByBarcodeAsync(string barcode)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.JanCode == barcode);
    }
}
