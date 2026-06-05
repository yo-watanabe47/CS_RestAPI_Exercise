using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Application.Tests.Usecase.Products.Interactors;
/// <summary>
/// ユースケース:[商品を変更する]を実現するインターフェイスの実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Usecase/Products/Interactor")]
public class UpdateProductUsecaseTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private IUpdateProductUsecase? _usecase;
    // 商品リポジトリ
    private IProductRepository? _repository;

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
            .AddJsonFile("appsettings.json", optional: false).Build();
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
        _usecase =
        _scope.ServiceProvider.GetRequiredService<IUpdateProductUsecase>();
        // 商品リポジトリを取得する
        _repository =
        _scope.ServiceProvider.GetRequiredService<IProductRepository>();
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

    [TestMethod("存在する商品Idで商品を取得できる")]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenIdExists()
    {
        var result = await _usecase!.GetProductByIdAsync("79023e82-9197-40a5-b236-26487f404be4");
        // nullでないことを検証する
        Assert.IsNotNull(result);
        // 商品Idを検証する
        Assert.AreEqual("79023e82-9197-40a5-b236-26487f404be4", result.ProductUuid);
        // 商品名を検証する
        Assert.AreEqual("油性ボールペン(赤)", result.Name);
        // 単価を検証する
        Assert.AreEqual(120, result.Price);
        // 商品在庫Idを検証する
        Assert.AreEqual("ff01a4b2-822e-462a-b200-2165bd07c618", result.Stock!.StockUuid);
        // 商品在庫数を検証する
        Assert.AreEqual(100, result.Stock!.Stock);
        // 商品カテゴリIdを検証する
        Assert.AreEqual("e3e5bdff-7132-44dc-a631-dc9576ad8b39", result.Category!.CategoryUuid);
        // 商品カテゴリ名を検証する
        Assert.AreEqual("文房具", result.Category!.Name);
    }

    [TestMethod("存在しない商品Idの場合、NotFoundExceptionがスローされる")]
public async Task GetProductByIdAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
{
    var ex = await Assert.ThrowsExactlyAsync<NotFoundException>(async () =>
    {
        await _usecase!.GetProductByIdAsync("79023e82-9197-40a5-b236-26487f404be5");
    });
    // nullでないことを検証する
    Assert.IsNotNull(ex);
    // 例外メッセージを検証する
    Assert.AreEqual("商品Id:79023e82-9197-40a5-b236-26487f404be5の商品は存在しません。", ex.Message);
 }

 [TestMethod("存在しない商品名を指定すると例外はスローされない")]
public async Task ExistsByProductNameAsync_ShouldNotThrow_WhenNameExists()
{
    await _usecase!.ExistsByProductNameAsync("油性ボールペン");
    Assert.IsTrue(true);
}

[TestMethod("存在する商品名を指定するとExistsExceptionがスローされる")]
public async Task ExistsByProductNameAsync_ShouldThrowExistsException_WhenNameDoesNotExist()
{
    var ex = await Assert.ThrowsExactlyAsync<ExistsException>(async () =>
    {
        await _usecase!.ExistsByProductNameAsync("油性ボールペン(赤)");
    });
    Assert.AreEqual("商品名:油性ボールペン(赤)は既に存在します。", ex.Message);
}

    [TestMethod("商品の変更:存在する商品の場合、商品を変更できる")]
    public async Task UpdateProductAsync_ShouldUpdateProduct_WhenProductExists()
    {
        const string id = "79023e82-9197-40a5-b236-26487f404be4";
        // 変更データを用意する
        var product = new Product(id, "油性ボールペン(Red)", 130);
        var productStock = new ProductStock("ff01a4b2-822e-462a-b200-2165bd07c618", 150);
        product.ChangeStock(productStock);

        // 商品を変更する
        await _usecase!.UpdateProductAsync(product);

        // 変更データを取得する
        var changeProduct = await _repository!
            .SelectByIdWithProductStockAndProductCategoryAsync(id);
        // 商品名を検証する
        Assert.AreEqual("油性ボールペン(Red)", changeProduct!.Name);
        // 単価を検証する
        Assert.AreEqual(130, changeProduct!.Price);
        // 商品在庫を検証する
        Assert.AreEqual(150, changeProduct.Stock!.Stock);

        // クリーニング：変更データを復元する
        product.ChangeName("油性ボールペン(赤)");
        product.ChangePrice(120);
        product.Stock!.ChangeStock(100);
        await _usecase.UpdateProductAsync(product);
    }

        [TestMethod("商品の変更:存在しない商品Idの場合、NotFoundExceptionがスローされる")]
    public async Task UpdateProductAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
    {
        const string id = "79023e82-9197-40a5-b236-26487f404be5";
        // 変更データを用意する
        var product = new Product(id, "油性ボールペン(Red)", 130);
        var productStock = new ProductStock("828fc628-6f6b-11f0-954a-00155d1bd29a", 150);
        product.ChangeStock(productStock);
        var ex = await Assert.ThrowsExactlyAsync<NotFoundException>(async () =>
        {
            // 商品を変更する
            await _usecase!.UpdateProductAsync(product);
        });
        // nullでないことを検証する
        Assert.IsNotNull(ex);
        // 例外メッセージを検証する
        Assert.AreEqual("商品Id:79023e82-9197-40a5-b236-26487f404be5の商品は存在しないため変更できません。", ex.Message);
    }
}