using ConveniencePos.Models;

namespace ConveniencePos.Data.Seed;

/// <summary>
/// テスト用の商品マスタデータ。
/// MVP検証用に5商品を定義する。
/// </summary>
public static class ProductSeedData
{
    /// <summary>
    /// シードデータとして挿入する商品一覧を返す。
    /// </summary>
    public static Product[] GetProducts() =>
    [
        new Product
        {
            Id = 1,
            JanCode = "777777",
            Name = "おにぎり 梅",
            Price = 120m,
            TaxRate = 8
        },
        new Product
        {
            Id = 2,
            JanCode = "888888",
            Name = "緑茶 500ml",
            Price = 150m,
            TaxRate = 8
        },
        new Product
        {
            Id = 3,
            JanCode = "999999",
            Name = "ポテトチップス",
            Price = 180m,
            TaxRate = 10
        },
        new Product
        {
            Id = 4,
            JanCode = "111111",
            Name = "ティッシュ",
            Price = 200m,
            TaxRate = 10
        },
        new Product
        {
            Id = 5,
            JanCode = "222222",
            Name = "コーヒー 熱 350ml",
            Price = 110m,
            TaxRate = 10
        }
    ];
}
