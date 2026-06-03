using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Infrastructure.Contexts;
using RestAPI_Exercise.Infrastructure.Entities;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Contexts;
/// <summary>
/// アプリケーション用DbContextの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Contexts")]
public class AddDbContextTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private AppDbContext? _dbContext;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="context"></param>
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
        // 生成したDIコンテナを破棄する
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
        // テストターゲットを取得
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

    [TestMethod("データベース接続ができる")]
    public void DbConnect_ShouldSucceed()
    {
        try
        {
            // データベースに接続する
            // _dbContext!.Database.OpenConnection();
            var canConnect = _dbContext!.Database.CanConnect();
            _testContext?.WriteLine("DB接続成功しました。");
            Assert.IsTrue(canConnect, "データベースに接続できません。");
        }
        catch (Exception ex)
        {
            _testContext?.WriteLine($"例外が発生しました: {ex.Message}");
            _testContext?.WriteLine($"スタックトレース:\n{ex.StackTrace}");
            Assert.Fail("接続に失敗しました。");
        }
        /*finally
        {
            // データベース接続を解除する
            _dbContext!.Database.CloseConnection();
            _dbContext!.Dispose(); // AppDbContxetを破棄する
        }*/
    }

    [TestMethod("DbSetプロパティにアクセスできる")]
    public void DbSet_Properties_ShouldBeAccessible()
    {
        // DbSetプロパティにアクセスできることを検証する
        Assert.IsNotNull(_dbContext!.Products, "Products DbSet にアクセスできません。");
        Assert.IsNotNull(_dbContext!.ProductCategories, "ProductCategories DbSet にアクセスできません。");
        Assert.IsNotNull(_dbContext!.ProductStocks, "ProductStocks DbSet にアクセスできません。");

        // 型が期待どおりであることを検証する
        Assert.IsInstanceOfType(_dbContext!.Products, typeof(DbSet<ProductEntity>));
        Assert.IsInstanceOfType(_dbContext!.ProductCategories, typeof(DbSet<ProductCategoryEntity>));
        Assert.IsInstanceOfType(_dbContext!.ProductStocks, typeof(DbSet<ProductStockEntity>));

        // クエリが実行できることを検証する
        // データが空でも例外なくCount()が返ればOKとする
        try
        {
            var _ = _dbContext!.Products.Count(); // 例外が出ないことを確認
            var __ = _dbContext!.ProductCategories.Count();
            var ___ = _dbContext!.ProductStocks.Count();
            Assert.IsTrue(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"DbSetに対する基本的なクエリ実行に失敗: {ex.Message}");
        }
    }
}