namespace ConveniencePos.Models;

public class Product
{
    public int Id { get; set; }
    public string JanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TaxRate { get; set; }
}
