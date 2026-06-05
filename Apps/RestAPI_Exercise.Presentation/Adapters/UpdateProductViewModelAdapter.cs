using RestAPI_Exercise.Application.Domains.Adapters;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Presentation.ViewModels;
namespace RestAPI_Exercise.Presentation.Adapters;
/// <summary>
/// UpdateProductViewModelからドメインオブジェクト:Productへ変換するアダプタ
/// </summary>
public class UpdateProductViewModelAdapter : IRestorer<Product, UpdateProductViewModel>
{
    /// <summary>
    /// UpdateProductViewModelからドメインオブジェクト:Productを復元する
    /// </summary>
    /// <param name="target">ユースケース:[商品を変更する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<Product> RestoreAsync(UpdateProductViewModel target)
    {
        // 商品在庫を生成する
        var productStock = new ProductStock(target.Stock);
        // 商品を生成する
        var product = new Product(target.ProductId, target.Name, target.Price);
        // 商品在庫を設定する
        product.ChangeStock(productStock);
        return Task.FromResult(product);
    }
}