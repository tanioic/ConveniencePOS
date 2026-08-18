using ConveniencePos.Data.Seed;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;

namespace ConveniencePos.Data;

public class PosDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

    protected PosDbContext() { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasIndex(p => p.JanCode).IsUnique();
        modelBuilder.Entity<Product>().HasData(ProductSeedData.GetProducts());
    }
}
