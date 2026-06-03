using RestAPI_Exercise.Application.Domains.Adapters;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Entities;
namespace RestAPI_Exercise.Infrastructure.Adapters;
/// <summary>
/// ドメインオブジェクト:ProductとProductEntityの相互変換クラス
/// </summary> 
/// <typeparam name="Product">ドメインオブジェクト:Product</typeparam>
/// <typeparam name="ProductEntity">EFCore:ProductEntity</typeparam>
public class ProductEntityAdapter :
IConverter<Product, ProductEntity>, IRestorer<Product, ProductEntity>
{
    /// <summary>
    /// ドメインオブジェクト:ProductをProductEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:Product</param>
    /// <returns>EFCore:ProductEntity</returns>
    public Task<ProductEntity> ConvertAsync(Product domain)
    {
        // 引数domainがnullの場合
        _ = domain ?? throw new InternalException("引数domainがnullです。");
        // ドメインオブジェクト:DepartmentをDepartmentEntityに変換する
        var entity = new ProductEntity();
        entity.ProductUuid = domain.ProductUuid;
        entity.Name = domain.Name;
        entity.Price = domain.Price;
        return Task.FromResult(entity);
    }

    /// <summary>
    /// ProductEntityからドメインオブジェクト:Productを復元する
    /// </summary>
    /// <param name="target">>EFCore:ProductEntity</param>
    /// <returns>ドメインオブジェクト:Product</returns>
    public Task<Product> RestoreAsync(ProductEntity target)
    {
        // 引数targetがnullの場合
        _ = target ?? throw new InternalException("引数targetがnullです。");
        // ProductEntityからドメインオブジェクト:Productを復元する
        var domain = new Product(target.ProductUuid, target.Name, target.Price);
        return Task.FromResult(domain);
    }
}