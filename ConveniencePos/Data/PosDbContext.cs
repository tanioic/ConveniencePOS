using ConveniencePos.Data.Seed;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;

namespace ConveniencePos.Data;

/// <summary>
/// POSシステムのデータベースコンテキスト。
/// Entity Framework Core を使用して SQL Server に接続する。
/// </summary>
public class PosDbContext : DbContext
{
    /// <summary>商品マスタテーブル</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>取引テーブル</summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>取引明細テーブル</summary>
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    /// <summary>
    /// DIコンテキストからインスタンスを生成するコンストラクタ。
    /// </summary>
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

    /// <summary>
    /// マイグレーション用の空コンストラクタ。
    /// </summary>
    protected PosDbContext() { }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(ProductSeedData.GetProducts());
    }
}
