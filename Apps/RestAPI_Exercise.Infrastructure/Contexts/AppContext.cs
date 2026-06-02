using Microsoft.EntityFrameworkCore;
using RestAPI_Exercise.Infrastructure.Entities;
namespace RestAPI_Exercise.Infrastructure.Contexts;
/// <summary>
/// アプリケーション用データベースコンテキスト（PostgreSQL対応）
/// 方針：
/// - Product : ProductCategory = N:1（カテゴリ削除で商品も削除：Cascade）
/// - Product : ProductStock    = 1:1（商品削除で在庫も削除：Cascade）
/// - UUIDはドメイン層で生成して保存（DB側での自動生成はしない）
/// - 1:1は product_stock.product_id の一意制約で担保
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options"></param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    /// <summary>
    /// 商品テーブルアクセスプロパティ
    /// </summary>
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    /// <summary>
    /// 商品カテゴリテーブルアクセスプロパティ
    /// </summary>
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    /// <summary>
    /// 商品在庫テーブルアクセスプロパティ
    /// </summary>
    public DbSet<ProductStockEntity> ProductStocks => Set<ProductStockEntity>();

    // TODO: Fluent API でマッピングを定義する
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 商品テーブルに対する動作設定
        modelBuilder.Entity<ProductEntity>(e =>
        {
            // 商品Id(UUID)はユニーク
            e.HasIndex(p => p.ProductUuid).IsUnique();
            // 商品名はvarchar(30)でNULL許容
            e.Property(p => p.Name).HasMaxLength(30);
            // 商品カテゴリと商品のカーディナリティ(1:N) 商品カテゴリ削除時に商品も削除
            e.HasOne(p => p.ProductCategory)
                .WithMany(c => c.Products!)
                .HasForeignKey(p => p.ProductCategoryId)
                .HasConstraintName("product_ibfk_category")
                .OnDelete(DeleteBehavior.Cascade);
            // 商品と商品在庫のカーディナリティ(1:1) 商品削除時に商品在庫も削除
            e.HasOne(p => p.ProductStock)
                .WithOne(s => s.Product!)
                .HasForeignKey<ProductStockEntity>(s => s.ProductId)
                .HasConstraintName("product_stock_ibfk_product")
                .OnDelete(DeleteBehavior.Cascade);
            // C#のstring ⇔ PostgreSQLのuuidを自動変換する
            e.Property(p => p.ProductUuid)
             .HasConversion(
                 v => Guid.Parse(v),
                 v => v.ToString()
            );
        });
        // 商品カテゴリの動作設定
        modelBuilder.Entity<ProductCategoryEntity>(e =>
        {
            // 商品カテゴリId(UUID)はユニーク
            e.HasIndex(c => c.CategoryUuid).IsUnique();
            // 商品カテゴリ名はvarchar(30)でNULL許容
            e.Property(c => c.Name).HasMaxLength(30);

            // C#のstring ⇔ PostgreSQLのuuidを自動変換する
            e.Property(c => c.CategoryUuid)
             .HasConversion(
                 v => Guid.Parse(v),  // C#(string)をDB(uuid)に書き込む時の処理
                 v => v.ToString()    // DB(uuid)をC#(string)に読み込む時の処理
            );
        });
        // 商品在庫の動作設定
        modelBuilder.Entity<ProductStockEntity>(e =>
        {
            // 商品在庫Id(UUID)はユニーク
            e.HasIndex(s => s.StockUuid).IsUnique();
            // 商品Id(UUID)はユニーク
            e.HasIndex(s => s.ProductId).IsUnique();
            // C#のstring ⇔ PostgreSQLのuuidを自動変換する
            e.Property(s => s.StockUuid)
             .HasConversion(
                 v => Guid.Parse(v),
                 v => v.ToString()
            );
        });
    }
}