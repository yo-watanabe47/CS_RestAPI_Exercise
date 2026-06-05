using Microsoft.EntityFrameworkCore;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Contexts;
namespace RestAPI_Exercise.Infrastructure.Repositories;
/// <summary>
///  ドメインオブジェクト:商品のCRUD操作インターフェイスの実装
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ProductFactory _factory;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">アプリケーション用データベースコンテキスト</param>
    /// <param name="factory">商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラス</param>
    public ProductRepository(AppDbContext context, ProductFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// 商品を永続化する
    /// </summary>
    /// <param name="product">永続化する商品</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(Product product)
    {
        try
        {
            // 登録する商品の商品カテゴリを取得する
            var category = await _context.ProductCategories
                .SingleOrDefaultAsync(c => c.CategoryUuid == product.Category!.CategoryUuid);
            if (category is null)
            {
                throw new Exception($"Id:{product.Category!.CategoryUuid}の商品カテゴリは存在しません。");
            }
            // ProductをProductEntityに変換する
            var entity = await _factory.ConvertAsync(product);
            // 商品カテゴリの外部キーを設定する
            entity.ProductCategory = null;
            entity.ProductCategoryId =category.Id;
            // 商品を登録する
            await _context.Products.AddAsync(entity);
            // 登録した商品をデータベースに永続化する
            await _context.SaveChangesAsync();
        }
        catch (DomainException)
        {
            throw; // DomainException例外はそのまま再スローする
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException("商品の永続化中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品Idの商品と在庫、商品カテゴリを返す
    /// </summary>
    /// <param name="id">商品Id</param>
    /// <returns>Product または null</returns>
    public async Task<Product?> SelectByIdWithProductStockAndProductCategoryAsync(string id)
    {
        try
        {
            // 商品Id(UUID)で商品と在庫、商品カテゴリをジョインして取得する
            var entity = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductStock)
                .SingleOrDefaultAsync(p => p.ProductUuid == id);
            if (entity is null)
            {
                return null; // 該当商品が存在しない場合はnullを返す
            }
            // ProductEntityの集約からProductの集約に復元する
            var product = await _factory.RestoreAsync(entity);
            return product;
        }
        catch (DomainException)
        {
            throw; // DomainException例外はそのまま再スローする
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の商品取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定されたキーワードで商品を部分一致検索して商品と在庫、商品カテゴリを取得する
    /// </summary>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>Prodyctのリスト</returns>
    public async Task<List<Product>> SelectByNameLikeWithProductStockAndProductCategoryAsync(string keyword)
    {
        try
        {
            // 引数のキーワードで商品と在庫を部分一致検索する
            var entities = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductStock)
                .Include(p => p.ProductCategory)
                .Where(p => EF.Functions.Like(p.Name, $"%{keyword}%"))
                .ToListAsync();
            // List<ProductEntity>からList<Product>を復元する
            var products = await _factory.RestoreAsync(entities);
            return products;
        }
        catch (DomainException)
        {
            throw; // DomainException例外はそのまま再スローする
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"キーワード:{keyword}の商品取得時に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 商品を更新する
    /// </summary>
    /// <param name="product">更新対象の商品</param>
    /// <returns>true:更新成功 false:更新失敗</returns>
    public async Task<bool> UpdateByIdAsync(Product product)
    {
        try
        {
            var entity = await _context.Products
            .Include(p => p.ProductStock)
            .SingleOrDefaultAsync(p => p.ProductUuid == product.ProductUuid);
            if (entity is null)
            {
                return false;
            }
            // 商品名と単価を変更する
            entity.Name = product.Name;
            entity.Price = product.Price;
            // 在庫数を変更する
            entity.ProductStock!.Stock = product.Stock!.Stock;
            // 変更データをデータベースに永続化する
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{product.ProductUuid}の商品変更中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 商品を削除する
    /// </summary>
    /// <param name="id">削除対象の商品Id(UUID)</param>
    /// <returns>true:削除成功 false:削除失敗</returns>
    public async Task<bool> DeleteByIdAsync(string id)
    {
        try
        {
            // 削除対象の商品を取得する
            var entity = await _context.Products.SingleOrDefaultAsync(p => p.ProductUuid == id);
            if (entity is null)
            {
                return false; // 該当商品が存在しない場合はfalseを返す
            }
            // 商品を削除する
            _context.Products.Remove(entity);
            // 削除結果をデータベースに反映させる
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の商品削除中に予期しないエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 指定された商品名の存在有無を返す
    /// </summary>
    /// <param name="name">商品名</param>
    /// <returns>true:存在する false:存在しない</returns> 
    public async Task<bool> ExistsByNameAsync(string name)
    {
        try
        {
            return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Name == name);
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Name:{name}の商品有無取得時に予期しないエラーが発生しました。", ex);
        }
    }
}