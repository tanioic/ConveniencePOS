using ConveniencePos.Data;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConveniencePos.Services;

/// <summary>
/// バーコードJANコードによる商品検索の実装。
/// IDbContextFactory を使用してスレッドセーフにDB接続を管理する。
/// </summary>
public class BarcodeService : IBarcodeService
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly ILogger<BarcodeService> _logger;

    /// <summary>
    /// コンストラクタ。DIコンテナからファクトリとロガーを受け取る。
    /// </summary>
    /// <param name="contextFactory">DBコンテキストファクトリ。</param>
    /// <param name="logger">ロガー。</param>
    public BarcodeService(IDbContextFactory<PosDbContext> contextFactory, ILogger<BarcodeService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Product?> LookupByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(barcode);

        _logger.LogDebug("バーコード検索: {Barcode}", barcode);

        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.JanCode == barcode, cancellationToken);
    }
}
