using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Entities;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Adapters;
/// <summary>
/// ドメインオブジェクト:ProductとProductEntityの相互変換クラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class ProductEntityAdapterTests
{
    // テストターゲット
    private ProductEntityAdapter _adapter = null!;
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
        // コープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter =
        _scope.ServiceProvider.GetRequiredService<ProductEntityAdapter>();  
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        // コープドサービスを破棄する
        _scope!.Dispose();
    }

    [TestMethod("ProductからProductEntityに変換できる")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly()
    {
        // 変換対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var domain = new Product(uuid, "ペン", 120);
        // ProductをProductEntityに変換する
        var entity = await _adapter.ConvertAsync(domain);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 商品Idが一致することを検証する
        Assert.AreEqual(uuid, entity.ProductUuid);
        // 商品名がペンであることを検証する
        Assert.AreEqual("ペン", entity.Name);
        // 単価が120であることを検証する
        Assert.AreEqual(120, entity.Price);
    }

    [TestMethod("ConvertAsync()メソッドにnullを渡すとInternalExceptionをスローする")]
    public async Task ConvertAsync_Should_ThrowException_When_Null()
    {
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            _ = await _adapter.ConvertAsync(null!);
        });
        Assert.AreEqual("引数domainがnullです。", ex.Message);
    }

    [TestMethod("ProductEntityからProductを復元できる")]
    public async Task RestoreAsync_Should_MapPropertiesCorrectly()
    {
        // 復元対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var entity = new ProductEntity { ProductUuid = uuid, Name = "ノート", Price = 350 };
        // ProductEntityからProductを復元する
        var domain = await _adapter.RestoreAsync(entity);
        // nullでないことを検証する
        Assert.IsNotNull(domain);
        // 商品Idが一致していることを検証する
        Assert.AreEqual(uuid, domain.ProductUuid);
        // 商品名がノートであることを検証する
        Assert.AreEqual("ノート", domain.Name);
        // 単価が350であることを検証する
        Assert.AreEqual(350, domain.Price);
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