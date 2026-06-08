using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Infrastructure.Contexts;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Repositories;
/// <summary>
///  ドメインオブジェクト:商品のCRUD操作インターフェイスの実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class ProductRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // アプリケーションで利用するDbContextの継承
    private  AppDbContext? _dbContext;
    // テストターゲット
    private IProductRepository _productRepository = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="_"></param>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // MSTestテスト用ログ出力ハンドルを設定する
        _testContext = context;
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
        _productRepository =
        _scope.ServiceProvider.GetRequiredService<IProductRepository>();
        // AppDbContxetを取得する
        _dbContext =
        _scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

    [TestMethod("存在する商品Idで商品、商品在庫、商品カテゴリを取得できる")]
    public async Task SelectByIdWithProductStockAndProductCategoryAsync_WhenIdExists_ShouldReturnProductWithStockAndCategory()
    {
        var product = await _productRepository
        .SelectByIdWithProductStockAndProductCategoryAsync("8f81a72a-58ef-422b-b472-d982e8665292");
        // nullでないことを検証する
        Assert.IsNotNull(product);
        // 商品Idを検証する
        Assert.AreEqual("8f81a72a-58ef-422b-b472-d982e8665292", product.ProductUuid);
        // 商品名を検証する
        Assert.AreEqual("水性ボールペン(赤)", product.Name);
        // 単価を検証する
        Assert.AreEqual(100, product.Price);
        // 商品在庫がnullでないことを検証する
        Assert.IsNotNull(product.Stock);
        // 商品在庫Idを検証する
        Assert.AreEqual("684b2cb1-7c4d-4b56-9975-e7facd06ab23", product.Stock.StockUuid);
        // 在庫数を検証する
        Assert.AreEqual(10, product.Stock.Stock);
        // 商品カテゴリIdを検証する
        Assert.AreEqual("e3e5bdff-7132-44dc-a631-dc9576ad8b39", product.Category!.CategoryUuid);
        // 商品カテゴリ名を検証する
        Assert.AreEqual("文房具", product.Category!.Name);
    }

    [TestMethod("存在しない商品Idの場合nullが返される")]
    public async Task SelectByIdWithProductStockAndProductCategoryAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        var product = await _productRepository
        .SelectByIdWithProductStockAndProductCategoryAsync("8f81a72a-58ef-422b-b472-d982e8665282");
        // nullであることを検証する
        Assert.IsNull(product);
    }


        [TestMethod("商品と商品在庫を永続化できる")]
    public async Task CreateAsync_WithStock_ShouldPersistBoth()
    {
        // 登録データを用意する
        var productCategory = new ProductCategory("e3e5bdff-7132-44dc-a631-dc9576ad8b39", "文房具");
        var productStock = new ProductStock(Guid.NewGuid().ToString(), 20);
        var product = new Product(Guid.NewGuid().ToString(), "商品-A", 300);
        product.ChangeStock(productStock);
        product.ChangeCategory(productCategory);

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy!.ExecuteAsync(async () =>
        {
            // トランザクションを開始する
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                // 商品と商品在庫を永続化する
                await _productRepository.CreateAsync(product);
                // 登録された商品と商品在庫を取得して値を検証する
                var result = await _productRepository
                     .SelectByIdWithProductStockAndProductCategoryAsync(product.ProductUuid);
                 // nullでないことを検証する
                 Assert.IsNotNull(result);
                 // 商品Idを検証する
                 Assert.AreEqual(result.ProductUuid, product.ProductUuid);
                 // 商品名を検証する
                 Assert.AreEqual(result.Name, product.Name);
                 // 単価を検証する
                 Assert.AreEqual(result.Price, product.Price);
                 // 商品在庫がnullでないことを検証する
                 Assert.IsNotNull(result.Stock);
                 // 商品在庫Idを検証する
                 Assert.AreEqual(result.Stock.StockUuid, product.Stock!.StockUuid);
                 // 在庫数を検証する
                 Assert.AreEqual(result.Stock.Stock, product.Stock.Stock);
            }
            finally
            {
                tx.Rollback(); // トランザクションをロールバックする
                tx.Dispose();  // トランザクションリソースを開放する
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }


        [TestMethod("商品名が存在するとtrueが返される")]
    public async Task ExistsByName_WhenNameExists_ShouldReturnTrue()
    {
        var result = await _productRepository.ExistsByNameAsync("蛍光ペン(赤)");
        Assert.IsTrue(result);
    }

    [TestMethod("商品名が存在しないとfalseが返される")]
    public async Task ExistsByName_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        var result = await _productRepository.ExistsByNameAsync("蛍光ペン(黒)");
    Assert.IsFalse(result);
    }


       [TestMethod("存在する商品のキーワードを指定すると、該当する商品のリストが返される")]
    public async Task SelectByNameLikeWithProductStockAndProductCategoryAsync_WithExistingKeyword_ShouldReturnMatchingProducts()
    {
        var products = await _productRepository
        .SelectByNameLikeWithProductStockAndProductCategoryAsync("蛍光ペン");
        // nullでないことを検証する
        Assert.IsNotNull(products);
        // 件数が4件であることを検証する
        Assert.AreEqual(4, products.Count);
    }
    [TestMethod("存在しない商品のキーワードを指定すると、空の商品のリストが返される")]
    public async Task SelectByNameLikeWithProductStockAndProductCategoryAsync_WithNonExistingKeyword_ShouldReturnEmptyList()
    {
        var products = await _productRepository
            .SelectByNameLikeWithProductStockAndProductCategoryAsync("商品-X");
        // nullでないことを検証する
        Assert.IsNotNull(products);
        // 件数が0であることを検証する
        Assert.AreEqual(0, products.Count);
    }


        [TestMethod("存在する商品を変更するとtrueが返される")]
    public async Task UpdateProduct_WhenProductExists_ShouldReturnTrue()
    {
        // 変更データを準備する
        var productStock = new ProductStock("80ad9abf-5575-454a-bc7d-3068fa3077e8", 50);
        var product = new Product("ac413f22-0cf1-490a-9635-7e9ca810e544", "水性ボールペン(黒)", 150);
        product.ChangeStock(productStock);

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy!.ExecuteAsync(async () =>
        {
            // トランザクションを開始する
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                // 商品を変更する
                var result = await _productRepository.UpdateByIdAsync(product);
                // trueであることを検証する
                Assert.IsTrue(result);
                // 変更された商品を取得する
                var updateResult = await _productRepository
                    .SelectByIdWithProductStockAndProductCategoryAsync(product.ProductUuid);
                // 商品名を検証する
                Assert.AreEqual(product.Name, updateResult!.Name);
                // 単価を検証する
                Assert.AreEqual(product.Price, updateResult!.Price);
                // 商品在庫数を検証する
                Assert.AreEqual(product.Stock!.Stock, updateResult.Stock!.Stock);
        }
        finally
        {
            tx.Rollback(); // トランザクションをロールバックする
            tx.Dispose();  // トランザクションリソースを開放する
            _testContext!.WriteLine("トランザクションをロールバックしました。");
        } 
    });
}

    [TestMethod("存在しない商品を変更するとfalseが返される")]
    public async Task UpdateProduct_WhenProductDoesNotExist_ShouldReturnFalse()
    {
        // 変更データを準備する
        var productStock = new ProductStock("828fb567-6f6b-11f0-954a-00155d1bd30a", 50);
        var product = new Product("ac413f22-0cf1-490a-9635-7e9ca810e555", "ボールペン(黒)", 150);
        product.ChangeStock(productStock);
        // 商品を変更する
        var result = await _productRepository.UpdateByIdAsync(product);
        // falseが返されることを検証する
        Assert.IsFalse(result);
    }
}