using Microsoft.EntityFrameworkCore;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Contexts;
namespace RestAPI_Exercise.Infrastructure.Repositories;
/// <summary>
///  ドメインオブジェクト:商品カテゴリのCRUD操作インターフェイスの実装
/// </summary>
public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly ProductCategoryEntityAdapter _adapter;
    /// <summary>
    /// コンストラクタ 
    /// </summary>
    /// <param name="context">アプリケーション用データベースコンテキスト</param>
    /// <param name="adapter">ドメインオブジェクト:ProductCategoryとProductCategoryEntityの相互変換クラス</param> 
    public ProductCategoryRepository(
        AppDbContext context,
        ProductCategoryEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>ProductCategoryのリスト</returns>
    public async Task<List<ProductCategory>> SelectAllAsync()
    {
        try
        {
            // すべての商品カテゴリを取得する
            var entities = await _context.ProductCategories
                .AsNoTracking().ToListAsync();
            // ProductCategoryのリストを生成する
            var categories = new List<ProductCategory>();
            foreach (var entity in entities)
            {
                // ProductCategoryEntityからProductCategoryを復元する
                categories.Add(await _adapter.RestoreAsync(entity));
            }
            return categories;
        }
        catch (DomainException)
        {
            throw; // DomainException例外はそのまま再スローする
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException("すべての商品カテゴリ取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品カテゴリIdの商品カテゴリを取得する
    /// </summary>
    /// <param name="id">商品カテゴリId</param>
    /// <returns>ProductCategory または null</returns>
    public async Task<ProductCategory?> SelectByIdAsync(string id)
    {
        try
        {
            // 引数のUUIDで商品カテゴリを取得する
            var entity = await _context.ProductCategories
                .SingleOrDefaultAsync(c => c.CategoryUuid == id);
            if (entity is null)
            {
                return null; // 存在しない場合はnullを返す
            }
            // ProductCategoryEntityからProductCategoryを復元する
            var category = await _adapter.RestoreAsync(entity);
            return category;
        }
        catch (DomainException)
        {
            throw; // DomainException例外はそのまま再スローする
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の商品カテゴリ取得時に予期しないエラーが発生しました。", ex);
        }
    }
}