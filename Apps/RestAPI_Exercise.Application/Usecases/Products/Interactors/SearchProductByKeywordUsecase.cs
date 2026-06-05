using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
namespace RestAPI_Exercise.Application.Usecases.Products.Interactors;
/// <summary>
/// ユースケース:[商品をキーワード検索する]を実現するインターフェイスの実装
/// </summary>
public class SearchProductByKeywordUsecase : ISearchProductByKeywordUsecase
{
    private readonly IProductRepository _repository;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">商品CRUD操作リポジトリ</param>
    public SearchProductByKeywordUsecase(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 指定されたキーワードで商品を部分一致検索した結果を返す
    /// </summary>
    /// <param name="keyword">商品キーワード</param>
    /// <returns>キーワード検索結果</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<List<Product>> ExecuteAsync(string keyword)
    {
        var result = await _repository
            .SelectByNameLikeWithProductStockAndProductCategoryAsync(keyword);
        return result;
    }
}