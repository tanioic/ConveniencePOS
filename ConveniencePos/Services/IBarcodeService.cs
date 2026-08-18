using ConveniencePos.Models;

namespace ConveniencePos.Services;

/// <summary>
/// バーコードJANコードによる商品検索を行うサービスのインターフェース。
/// </summary>
public interface IBarcodeService
{
    /// <summary>
    /// JANコードに合致する商品をDBから検索する。
    /// </summary>
    /// <param name="barcode">JANコード（バーコード文字列）</param>
    /// <returns>該当商品。存在しなければnull。</returns>
    Task<Product?> LookupByBarcodeAsync(string barcode);
}
