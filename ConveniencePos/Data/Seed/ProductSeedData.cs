using ConveniencePos.Models;

namespace ConveniencePos.Data.Seed;

public static class ProductSeedData
{
    public static Product[] GetProducts() =>
    [
        new Product
        {
            Id = 1,
            JanCode = "777777",
            Name = "おにぎり 梅",
            Price = 120m
        },
        new Product
        {
            Id = 2,
            JanCode = "888888",
            Name = "緑茶 500ml",
            Price = 150m
        },
        new Product
        {
            Id = 3,
            JanCode = "999999",
            Name = "ポテトチップス",
            Price = 180m
        },
        new Product
        {
            Id = 4,
            JanCode = "111111",
            Name = "サンドイッチ たまご",
            Price = 250m
        },
        new Product
        {
            Id = 5,
            JanCode = "222222",
            Name = "コーヒー 熱 350ml",
            Price = 110m
        }
    ];
}
