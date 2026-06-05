using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.ViewModels;

namespace RestAPI_Exercise.Presentation.Tests.Adapters;

/// <summary>
/// UpdateProductViewModelAdapter のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class UpdateProductViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private UpdateProductViewModelAdapter? _adapter;

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
    /// テストメソッド実行の前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter = _scope.ServiceProvider
            .GetRequiredService<UpdateProductViewModelAdapter>();
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

    [TestMethod("ViewModelから既存Productを復元できる")]
    public async Task RestoreAsync_ShouldMapVmToDomain_ForExistingProduct()
    {
        // ViewModelを用意する
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = "シャープペンシル",
            Price = 220,
            Stock = 10
        };
        // ViewModelからProductを復元する
        var product = await _adapter!.RestoreAsync(viewModel);
        // nullでないことを検証する
        Assert.IsNotNull(product);
        // 商品Idを検証する
        Assert.AreEqual(viewModel.ProductId, product.ProductUuid);
        // 商品名を検証する
        Assert.AreEqual(viewModel.Name, product.Name);
        // 単価を検証する
        Assert.AreEqual(viewModel.Price, product.Price);
        // 商品在庫がnullでないことを検証する
        Assert.IsNotNull(product.Stock);
        // 商品在庫数を検証する
        Assert.AreEqual(viewModel.Stock, product.Stock!.Stock);
    }

    [TestMethod("商品Idが不正なUUID形式の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenProductIdInvalidUuid()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = "NOT-A-UUID",
            Name = "ノート",
            Price = 200,
            Stock = 1
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("商品名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameBlank_ShouldThrowDomainException()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = " ",
            Price = 100,
            Stock = 1
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("商品名は必須です。", ex.Message);
    }
    [TestMethod("商品名が31文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameOver30_ShouldThrowDomainException()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = new string('A', 31),
            Price = 100,
            Stock = 1
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("商品名は30文字以内である必要があります。", ex.Message);
    }

    [TestMethod("商品名が30文字ちょうどは復元できる（境界値OK）")]
    public async Task RestoreAsync_WhenNameLengthIs30_ShouldSucceed()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = new string('A', 30),
            Price = 100,
            Stock = 1
        };
        // ViewModelからProductを復元する
        var product = await _adapter!.RestoreAsync(viewModel);
        // nullでないことを検証する
        Assert.IsNotNull(product);
        // 商品名の長さが30であることを検証する
        Assert.AreEqual(30, product.Name.Length);
    }

    [TestMethod("在庫数がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenStockIsNegative()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = "消しゴム",
            Price = 130,
            Stock = -1
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("在庫数は0以上である必要があります。", ex.Message);
    }

    [TestMethod("単価がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenPriceIsNegative()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = "消しゴム",
            Price = -10,
            Stock = 1
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("価格は0円以上である必要があります。", ex.Message);
    }
}