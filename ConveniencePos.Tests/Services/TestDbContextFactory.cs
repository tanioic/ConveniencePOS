using ConveniencePos.Data;
using Microsoft.EntityFrameworkCore;

namespace ConveniencePos.Tests.Services;

/// <summary>
/// テスト用の IDbContextFactory 実装。毎回新しい DbContext を生成する（InMemory DB名を共有）。
/// </summary>
internal class TestDbContextFactory : IDbContextFactory<PosDbContext>, IDisposable
{
    private readonly DbContextOptions<PosDbContext> _options;

    public TestDbContextFactory(DbContextOptions<PosDbContext> options)
    {
        _options = options;
    }

    public PosDbContext CreateDbContext() => new(_options);

    public ValueTask<PosDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(new PosDbContext(_options));

    public void Dispose() { }
}
