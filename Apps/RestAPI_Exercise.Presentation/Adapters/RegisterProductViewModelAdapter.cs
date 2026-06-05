using RestAPI_Exercise.Application.Domains.Adapters;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Presentation.ViewModels;
namespace RestAPI_Exercise.Presentation.Adapters;
/// <summary>
/// RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ
/// </summary>
public class RegisterProductViewModelAdapter : IRestorer<Product, RegisterProductViewModel>
{
    /// <summary>
    /// RegisterProductViewModelからドメインオブジェクト:Productを復元する
    /// </summary>
    /// <param name="target">ユースケース:[新商品を登録する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<Product> RestoreAsync(RegisterProductViewModel target)
    {
        // 商品カテゴリを生成する
        var category = new ProductCategory(target.CategoryId, target.CategoryName);
        // 商品在庫を生成する
        var productStock = new ProductStock(target.Stock);
        // 商品を生成する
        var product = new Product(Guid.NewGuid().ToString(), target.Name, target.Price);
        // 商品カテゴリと商品在庫を設定する
        product.ChangeCategory(category);
        product.ChangeStock(productStock);
        return Task.FromResult(product);
    }
}