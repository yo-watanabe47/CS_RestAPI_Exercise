using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Entities;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Adapters;
/// <summary>
/// 商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラスの単体テストドライバ
/// </summary>
[TestCategory("Adapters")]
[TestClass]
public class ProductFactoryTests
{
    // テストターゲット
    private ProductFactory _factory = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="_"></param>
    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        // アプリケーション管理を生成
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        // サービスプロバイダ(DIコンテナ)の生成    
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // 生成したサービスプロバイダ(DIコンテナ)を破棄する
        _provider?.Dispose();
    }   

    /// <summary>
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _factory =
        _scope.ServiceProvider.GetRequiredService<ProductFactory>();  
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        // スコープドサービスを破棄する
        _scope!.Dispose();
    }
[TestMethod("Productの集約からProductEntityの集約に変換できる(商品のみ)")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case1()
    {
        // 変換対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var product = new Product(uuid, "ペン", 120);
        // ProductをProductEntityに変換する
        var entity = await _factory.ConvertAsync(product);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 商品Idが一致することを検証する
        Assert.AreEqual(uuid, entity.ProductUuid);
        // 商品名がペンであることを検証する
        Assert.AreEqual("ペン", entity.Name);
        // 単価が120であることを検証する
        Assert.AreEqual(120, entity.Price);
    }

[TestMethod("Productの集約からProductEntityの集約に変換できる(商品、商品カテゴリ)")]
public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case2()
{
    // 変換対象を生成する
    var productUuid = Guid.NewGuid().ToString();
    var product = new Product(productUuid, "ペン", 120);
    var categoryUuid = Guid.NewGuid().ToString();
    var category = new ProductCategory(categoryUuid, "文房具");
    product.ChangeCategory(category);
    // ProductをProductEntityに変換する
    var entity = await _factory.ConvertAsync(product);
    // nullでないことを検証する
    Assert.IsNotNull(entity);
    // 商品Idが一致することを検証する
    Assert.AreEqual(productUuid, entity.ProductUuid);
    // 商品名がペンであることを検証する
    Assert.AreEqual("ペン", entity.Name);
    // 単価が120であることを検証する
    Assert.AreEqual(120, entity.Price);
    // 商品カテゴリIdが一致することを検証する
    Assert.AreEqual(categoryUuid, entity.ProductCategory!.CategoryUuid);
    // 商品カテゴリ名が一致することを検証する
    Assert.AreEqual("文房具", entity.ProductCategory!.Name);
}

[TestMethod("Productの集約からProductEntityの集約に変換できる(商品、商品カテゴリ、商品在庫)")]
public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case3()
{
    // 変換対象を生成する
    var productUuid = Guid.NewGuid().ToString();
    var product = new Product(productUuid, "ペン", 120);
    var categoryUuid = Guid.NewGuid().ToString();
    var category = new ProductCategory(categoryUuid, "文房具");
    var stockUuid = Guid.NewGuid().ToString();
    var stock = new ProductStock(stockUuid, 20);
    product.ChangeStock(stock);
    product.ChangeCategory(category);
    // ProductをProductEntityに変換する
    var entity = await _factory.ConvertAsync(product);
    // nullでないことを検証する
    Assert.IsNotNull(entity);
    // 商品Idが一致することを検証する
    Assert.AreEqual(productUuid, entity.ProductUuid);
    // 商品名がペンであることを検証する
    Assert.AreEqual("ペン", entity.Name);
    // 単価が120であることを検証する
    Assert.AreEqual(120, entity.Price);
    // 商品カテゴリIdが一致することを検証する
    Assert.AreEqual(categoryUuid, entity.ProductCategory!.CategoryUuid);
    // 商品カテゴリ名が一致することを検証する
    Assert.AreEqual("文房具", entity.ProductCategory!.Name);
    // 商品在庫Idが一致することを検証する
    Assert.AreEqual(stockUuid, entity.ProductStock!.StockUuid);
    // 商品在庫数が一致することを検証する
    Assert.AreEqual(20, entity.ProductStock!.Stock);
}

[TestMethod("ドメインオブジェクトのリストからエンティティのリストに変換できる")]
public async Task ConvertAsync_List_ShouldSucceed()
{
    // 変換対象リストを生成する
    var products = new List<Product>();
    var category = new ProductCategory(Guid.NewGuid().ToString(), "ボールペン(黒)");
    var product = new Product(Guid.NewGuid().ToString(), "ボールペン(黒)", 150);
    product.ChangeCategory(category);
    product.ChangeStock(new ProductStock(Guid.NewGuid().ToString(), 20));
    product = new Product(Guid.NewGuid().ToString(), "ボールペン(青)", 150);
    products.Add(product);
    product.ChangeCategory(category);
    product.ChangeStock(new ProductStock(Guid.NewGuid().ToString(), 10));
    products.Add(product);
    product = new Product(Guid.NewGuid().ToString(), "ボールペン(赤)", 150);
    product.ChangeCategory(category);
    product.ChangeStock(new ProductStock(Guid.NewGuid().ToString(), 30));
    products.Add(product);
    // List<Product>をList<ProductEntity>に変換する
    var entities = await _factory.ConvertAsync(products);
    // nullでないことを検証する
    Assert.IsNotNull(entities);
    // 件数を検証する
    Assert.AreEqual(3, entities.Count);
    // 保持している値を検証する
    var index = 0;
    foreach (var entity in entities)
    {
        // 商品Idが一致することを検証する
        Assert.AreEqual(products[index].ProductUuid, entity.ProductUuid);
        // 商品名が一致することを検証する
        Assert.AreEqual(products[index].Name, entity.Name);
        // 単価が一致することを検証する
       Assert.AreEqual(products[index].Price, entity.Price);
        // 商品カテゴリIdが一致することを検証する
        Assert.AreEqual(products[index].Category!.CategoryUuid,
            entity.ProductCategory!.CategoryUuid);
        // 商品カテゴリ名が一致することを検証する
        Assert.AreEqual(products[index].Category!.Name, entity.ProductCategory!.Name);
        // 商品在庫Idが一致することを検証する
        Assert.AreEqual(products[index].Stock!.StockUuid, entity.ProductStock!.StockUuid);
        // 商品在庫数が一致することを検証する
        Assert.AreEqual(products[index].Stock!.Stock, entity.ProductStock!.Stock);
        index++;
    }
}

[TestMethod("ProductEntityの集約からProductの集約を復元できる(商品のみ)")]
public async Task RestireAsync_Should_MapPropertiesCorrectly_Case1()
{
    // 変換対象を生成する
    var productEntity = new ProductEntity
   {
        ProductUuid = Guid.NewGuid().ToString(),
       Name = "ペン",
       Price = 120
    };
    // ProductEntityからproductを復元する
    var product = await _factory.RestoreAsync(productEntity);
    // nullでないことを検証する
   Assert.IsNotNull(product);
    // 商品Idが一致することを検証する
    Assert.AreEqual(productEntity.ProductUuid, product.ProductUuid);
    // 商品名が一致することを検証する
    Assert.AreEqual(productEntity.Name, product.Name);
    // 単価が一致することを検証する
    Assert.AreEqual(productEntity.Price, product.Price);
}

[TestMethod("ProductEntityの集約からProductの集約を復元できる(商品、商品カテゴリ)")]
public async Task RestireAsync_Should_MapPropertiesCorrectly_Case2()
{
    // 変換対象を生成する
    var productEntity = new ProductEntity
    {
        ProductUuid = Guid.NewGuid().ToString(),
        Name = "ペン",
        Price = 120,
        ProductCategory = new ProductCategoryEntity
        {
            CategoryUuid = Guid.NewGuid().ToString(),
            Name = "文房具"
        }
    };
    // ProductEntityからproductを復元する
    var product = await _factory.RestoreAsync(productEntity);

    // nullでないことを検証する
    Assert.IsNotNull(product);
    // 商品Idが一致することを検証する
    Assert.AreEqual(productEntity.ProductUuid, product.ProductUuid);
    // 商品名が一致することを検証する
    Assert.AreEqual(productEntity.Name, product.Name);
    // 単価が一致することを検証する
    Assert.AreEqual(productEntity.Price, product.Price);
    // 商品カテゴリIdが一致することを検証する
    Assert.AreEqual(productEntity.ProductCategory.CategoryUuid,
        product.Category!.CategoryUuid);
    // 商品カテゴリ名が一致することを検証する
    Assert.AreEqual(productEntity.ProductCategory.Name,
        product.Category!.Name);
}

[TestMethod("ProductEntityの集約からProductの集約を復元できる(商品、商品カテゴリ、商品在庫)")]
public async Task RestireAsync_Should_MapPropertiesCorrectly_Case3()
{
    // 変換対象を生成する
    var productEntity = new ProductEntity
    {
        ProductUuid = Guid.NewGuid().ToString(),
        Name = "ペン",
        Price = 120,
        ProductCategory = new ProductCategoryEntity
        {
            CategoryUuid = Guid.NewGuid().ToString(),
            Name = "文房具"
        },
        ProductStock = new ProductStockEntity
        {
            StockUuid = Guid.NewGuid().ToString(),
            Stock = 20
        }
    };

    // ProductEntityからproductを復元する
    var product = await _factory.RestoreAsync(productEntity);

    // nullでないことを検証する
    Assert.IsNotNull(product);
    // 商品Idが一致することを検証する
    Assert.AreEqual(productEntity.ProductUuid, product.ProductUuid);
    // 商品名が一致することを検証する
    Assert.AreEqual(productEntity.Name, product.Name);
    // 単価が一致することを検証する
    Assert.AreEqual(productEntity.Price, product.Price);
    // 商品カテゴリIdが一致することを検証する
    Assert.AreEqual(productEntity.ProductCategory.CategoryUuid,
        product.Category!.CategoryUuid);
    // 商品カテゴリ名が一致することを検証する
    Assert.AreEqual(productEntity.ProductCategory.Name,
        product.Category!.Name);
    // 商品在庫Idが一致することを検証する
    Assert.AreEqual(productEntity.ProductStock.StockUuid, product.Stock!.StockUuid);
    // 商品在庫数が一致することを検証する
    Assert.AreEqual(productEntity.ProductStock.Stock, product.Stock!.Stock);
}

[TestMethod("エンティティのリストからドメインオブジェクトのリストを復元できる")]
public async Task RestoreAsync_List_ShouldSucceed()
{
    var categoryEntity = new ProductCategoryEntity
    {
       CategoryUuid = Guid.NewGuid().ToString(),
        Name = "文房具"
    };
    var productEntities = new List<ProductEntity>();
    // 変換対象を生成する
    var productEntity = new ProductEntity
    {
        ProductUuid = Guid.NewGuid().ToString(),
        Name = "ボールペン(黒)",
        Price = 120,
        ProductCategory = categoryEntity,
        ProductStock = new ProductStockEntity
        {
            StockUuid = Guid.NewGuid().ToString(),
            Stock = 20
        }
    };
    productEntities.Add(productEntity);
    // 変換対象を生成する
    productEntity = new ProductEntity
    {
        ProductUuid = Guid.NewGuid().ToString(),
        Name = "ボールペン(青)",
        Price = 120,
        ProductCategory = categoryEntity,
        ProductStock = new ProductStockEntity
        {
            StockUuid = Guid.NewGuid().ToString(),
            Stock = 10
        }
    };
    productEntities.Add(productEntity);
    // 変換対象を生成する
    productEntity = new ProductEntity
    {
        ProductUuid = Guid.NewGuid().ToString(),
        Name = "ボールペン(赤)",
        Price = 120,
        ProductCategory = categoryEntity,
        ProductStock = new ProductStockEntity
        {
            StockUuid = Guid.NewGuid().ToString(),
            Stock = 30
        }
    };
    productEntities.Add(productEntity);
    // List<ProductEntity>からList<Product>を復元する
    var domains = await _factory.RestoreAsync(productEntities);
    // nullでないことを検証する
    Assert.IsNotNull(domains);
    // 件数を検証する
    Assert.AreEqual(3, domains.Count);
    // 保持している値を検証する
    var index = 0;
    foreach (var domain in domains)  
    {
        // 商品Idが一致することを検証する
        Assert.AreEqual(productEntities[index].ProductUuid, domain.ProductUuid);
        // 商品名が一致することを検証する
        Assert.AreEqual(productEntities[index].Name, domain.Name);   
        // 単価が一致することを検証する  
        Assert.AreEqual(productEntities[index].Price, domain.Price);    
        // 商品カテゴリIdが一致することを検証する
        Assert.AreEqual(productEntities[index].ProductCategory!.CategoryUuid,
            domain.Category!.CategoryUuid);
        // 商品カテゴリ名が一致することを検証する
        Assert.AreEqual(productEntities[index].ProductCategory!.Name,
            domain.Category!.Name);
        // 商品在庫Idが一致することを検証する
        Assert.AreEqual(productEntities[index].ProductStock!.StockUuid,
            domain.Stock!.StockUuid);
        // 商品在庫数が一致することを検証する
        Assert.AreEqual(productEntities[index].ProductStock!.Stock,
            domain.Stock!.Stock);
        index++;
    }
}




}