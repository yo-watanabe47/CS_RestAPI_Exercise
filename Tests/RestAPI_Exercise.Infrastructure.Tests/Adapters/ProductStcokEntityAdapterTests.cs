using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Entities;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Adapters;
/// <summary>
/// ドメインオブジェクト:ProductStockとProductStockEntity相互変換クラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class ProductStockEntityAdapterTests
{
    // テストターゲット
    private ProductStockEntityAdapter _adapter = null!;
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
        _adapter =
        _scope.ServiceProvider.GetRequiredService<ProductStockEntityAdapter>();  
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

    [TestMethod("ProductStockからProductStockEntityに変換できる")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly()
    {
        // ProductStockを生成する
        var stockUuid = Guid.NewGuid().ToString();
        var domain = new ProductStock(stockUuid, 25);
        // ProductStockをProductStockEntityに変換する
        var entity = await _adapter.ConvertAsync(domain);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 在庫IdがProductStcokと同じであるこを検証する
        Assert.AreEqual(stockUuid, entity.StockUuid);
        // 在庫数が25であることを検証する
        Assert.AreEqual(25, entity.Stock);
    }

    [TestMethod("ConvertAsync()メソッドに nullを渡すとInternalExceptionをスローする")]
    public async Task ConvertAsync_Should_ThrowException_When_Null()
    {
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            _ = await _adapter.ConvertAsync(null!);
        });
        Assert.AreEqual("引数domainがnullです。", exception.Message);
    }

    [TestMethod("ProductStockEntityからProductStockを復元できる")]
    public async Task RestoreAsync_Should_MapPropertiesCorrectly()
    {
        // ProductStcokEntityを生成する
        var stockUuid = Guid.NewGuid().ToString();
        var entity = new ProductStockEntity { StockUuid = stockUuid, Stock = 10 };
        // ProductStockEntityからProductStockを復元する
        var domain = await _adapter.RestoreAsync(entity);
        // nullでないことを検証する
        Assert.IsNotNull(domain);
        // 在庫Idが一致していることを検証する
        Assert.AreEqual(stockUuid, domain.StockUuid);
        // 在庫数が10であることを検証する
        Assert.AreEqual(10, domain.Stock);
    }

    [TestMethod("RestoreAsync()メソッドにnullを渡すとInternalExceptionをスローする")]
    public async Task RestoreAsync_Should_ThrowException_When_Null()
    {
        var exception = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            _ = await _adapter.RestoreAsync(null!);
        });
        Assert.AreEqual("引数targetがnullです。", exception.Message);        
    }
}