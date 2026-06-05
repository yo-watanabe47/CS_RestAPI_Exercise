using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.ViewModels;


namespace RestAPI_Exercise.Presentation.Tests.Adapters;
/// <summary>
/// RegisterProductViewModelAdapterのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class RegisterProductViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private RegisterProductViewModelAdapter? _adapter;

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
            .GetRequiredService<RegisterProductViewModelAdapter>();
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

    [TestMethod("ViewModelからProductを復元でき、商品Idと商品在庫Idが自動生成される")]
    public async Task RestoreAsync_ShouldMapVmToDomain_AndGenerateUuids()
    {
        // ViewModelを用意する
        var viewModel = new RegisterProductViewModel
        {
            Name = "消しゴム",
            Price = 130,
            Stock = 5,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // ViewModelからProductを復元する
        var product = await _adapter!.RestoreAsync(viewModel);
        // 商品名を検証する
        Assert.AreEqual(viewModel.Name, product.Name);
        // 単価を検証する
        Assert.AreEqual(viewModel.Price, product.Price);
        // 商品Idが生成されていることを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(product.ProductUuid));
        Assert.IsTrue(Guid.TryParse(product.ProductUuid, out _));
        // 商品カテゴリがnullでないことを検証する
        Assert.IsNotNull(product.Category);
        // 商品カテゴリIdを検証する
        Assert.AreEqual(viewModel.CategoryId, product.Category!.CategoryUuid);
        // 商品カテゴリ名を検証する
        Assert.AreEqual(viewModel.CategoryName, product.Category.Name);
        // 商品在庫がnullでないことを検証する
        Assert.IsNotNull(product.Stock);
        // 商品在庫を検証する
        Assert.AreEqual(viewModel.Stock, product.Stock!.Stock);
        // 商品在庫Idが生成されていることを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(product.Stock.StockUuid));
        Assert.IsTrue(Guid.TryParse(product.Stock.StockUuid, out _));
    }

    [TestMethod("不正な商品Idの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenCategoryIdIsInvalidUuid()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "ノート",
            Price = 200,
            Stock = 1,
            CategoryId = "NOT-A-UUID",
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("商品名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameBlank_ShouldThrowDomainException()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = " ",
            Price = 100,
            Stock = 1,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("商品名は必須です。", ex.Message);
    }

    [TestMethod("商品名が31文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameOver30_ShouldThrowDomainException()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = new string('A', 31),
            Price = 100,
            Stock = 1,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("商品名は30文字以内である必要があります。", ex.Message);
    }

    [TestMethod("カテゴリIdが空文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenCategoryIdIsEmpty()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "ペン",
            Price = 120,
            Stock = 1,
            CategoryId = "", // 空文字
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("在庫数がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenStockIsNegative()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "ノート",
            Price = 200,
            Stock = -1, // マイナス
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("在庫数は0以上である必要があります。", ex.Message);
    }

     [TestMethod("単価がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenPriceNegative()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "ノート",
            Price = -1,
            Stock = 0,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExactlyAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("価格は0円以上である必要があります。", ex.Message);
    }
}