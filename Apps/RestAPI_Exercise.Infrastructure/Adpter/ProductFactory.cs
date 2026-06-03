using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Infrastructure.Entities;

namespace RestAPI_Exercise.Infrastructure.Adapters;
/// <summary>
/// 商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラス
/// ドメインオブジェクト:ProductとProductEntityの相互変換
/// ドメインオブジェクト:ProductCategoryとProductEntityの相互変換
/// ドメインオブジェクト:ProductStockとProductStockEntityの相互変換
/// </summary>
public class ProductFactory
{
    private readonly ProductEntityAdapter _productEntityAdapter;
    private readonly ProductCategoryEntityAdapter _productCategoryEntityAdapter;
    private readonly ProductStockEntityAdapter _productStockEntityAdapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productEntityAdapter">ProductとProductEntityの相互変換</param>
    /// <param name="productCategoryEntityAdapter">ProductCategoryとProductEntityの相互変換</param>
    /// <param name="productStockEntityAdapter">ProductStockとProductStockEntityの相互変換</param>
    public ProductFactory(
        ProductEntityAdapter productEntityAdapter,
        ProductCategoryEntityAdapter productCategoryEntityAdapter,
        ProductStockEntityAdapter productStockEntityAdapter)
    {
        _productEntityAdapter = productEntityAdapter;
        _productCategoryEntityAdapter = productCategoryEntityAdapter;
        _productStockEntityAdapter = productStockEntityAdapter;
    }

    /// <summary>
    /// 商品、商品カテゴリ、商品在庫の集約関係を構築したEntityを生成して返す
    /// </summary>
    /// <param name="domain">ルートドメインオブジェクト:Product</param>
    /// <returns>集約関係を構築したProductEntity</returns>
    public async Task<ProductEntity> ConvertAsync(Product domain)
    {
        // ProductからProductEntityを生成する
        var entity = await _productEntityAdapter.ConvertAsync(domain);
        // 商品カテゴリ、在庫が存在しない場合はリターンする
        if (domain.Category is null && domain.Stock is null)
        {
            return entity;
        }
        // 商品カテゴリが存在する
        if (domain.Category != null)
        {
            // CategoryをCategoryEntityに変換してプロパティに設定する
            entity.ProductCategory =
                await _productCategoryEntityAdapter.ConvertAsync(domain.Category);
        }
        // 在庫が存在する
        if (domain.Stock != null)
        {
            // ProductStockをProductStockEntityに変換してプロパティに設定する
            entity.ProductStock =
                await _productStockEntityAdapter.ConvertAsync(domain.Stock);
        }
        return entity;
    }

    /// <summary>
    /// 商品、商品カテゴリ、商品在庫の集約関係を構築したEntityリストを生成して返す
    /// </summary>
    /// <param name="domains">ルートドメインオブジェクトのリスト:List<Product></param>
    /// <returns>集約関係を構築したProductEntityのリスト</returns>
    public async Task<List<ProductEntity>> ConvertAsync(List<Product> domains)
    {
        // ProductEntityのリストを生成する
        var entityies = new List<ProductEntity>();
        foreach (var domain in domains)
        {
            // リストから取り出したProductをProductEntityに変換してリストに追加する
            entityies.Add(await ConvertAsync(domain));
        }
        return entityies;
    }

    /// <summary>
    /// ProductEntityの集約関係からドメインオブジェクト:Productを復元する
    /// </summary>
    /// <param name="target">ProductEntity</param>
    /// <returns>復元したProduct</returns>
    public async Task<Product> RestoreAsync(ProductEntity target)
    {
        // ProductEntityからProductを復元する
        var product = await _productEntityAdapter.RestoreAsync(target);
        // 商品カテゴリ、商品在庫が存在しない場合はリターンする   
        if (target.ProductCategory is null && target.ProductStock is null)
        {
            return product;
        }
        // 商品カテゴリが存在する
        if (target.ProductCategory != null)
        {
            // ProductCategoryEntityからProductCategoryを復元してプロパティに設定する
            product.ChangeCategory(
                await _productCategoryEntityAdapter.RestoreAsync(target.ProductCategory));
        }
        // 商品在庫が存在する
        if (target.ProductStock != null)
        {
            // ProductStockEntityからProductStockを復元してプロパティに設定する
            product.ChangeStock(
                await _productStockEntityAdapter.RestoreAsync(target.ProductStock));
        }
        return product;
    }

    /// <summary>
    /// 商品、商品カテゴリ、商品アジ子の集約関係を構築したEntityリストからドメインオブジェクトのリストを復元する
    /// </summary>
    /// <param name="targets">List<ProductEntity></param>
    /// <returns>Product<List></returns>
    public async Task<List<Product>> RestoreAsync(List<ProductEntity> targets)
    {
        // Productのリストを生成する
        var products = new List<Product>();
        foreach (var target in targets)
        {
            // ProductEntityを取り出しProductを復元してリストに追加する
            products.Add(await RestoreAsync(target));
        }
        return products;
    }
}