using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Repositories;
/// <summary>
///  ドメインオブジェクト:商品カテゴリのCRUD操作インターフェイスの実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class ProductCategoryRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // テストターゲット
    private IProductCategoryRepository _productCategoryRepository = null!;
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
        _productCategoryRepository =
        _scope.ServiceProvider.GetRequiredService<IProductCategoryRepository>();  
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


    [TestMethod("すべての商品カテゴリを取得できる")]
    public async Task SelectAllAsync_ShouldReturnAllCategories()
    {
        // すべての商品カテゴリを取得する
        var categoryies = await _productCategoryRepository.SelectAllAsync();
        // nullでないことを検証する
        Assert.IsNotNull(categoryies);
        // 件数が3件であることを検証する
        Assert.AreEqual(3, categoryies.Count());
        // 取得内容を検証する
        Assert.AreEqual("e3e5bdff-7132-44dc-a631-dc9576ad8b39", categoryies[0].CategoryUuid);
        Assert.AreEqual("文房具", categoryies[0].Name);
        Assert.AreEqual("bab15a9d-fb32-44ff-9dde-dc7fa32237e0", categoryies[1].CategoryUuid);
        Assert.AreEqual("雑貨", categoryies[1].Name);
        Assert.AreEqual("21faa9b3-74c9-4539-a2dc-0039873ae654", categoryies[2].CategoryUuid);
        Assert.AreEqual("パソコン周辺機器", categoryies[2].Name);
        foreach (var category in categoryies)
        {
            _testContext?.WriteLine(category.ToString());
        }
    }

    [TestMethod("存在する商品カテゴリIdで商品カテゴリを取得できる")]
    public async Task SelectByIdAsync_WhenIdExists_ShouldReturnCategory()
    {
        var category = await _productCategoryRepository
            .SelectByIdAsync("e3e5bdff-7132-44dc-a631-dc9576ad8b39");
        // nullでないことを検証する
        Assert.IsNotNull(category);
        // 取得内容を検証する
        Assert.AreEqual("e3e5bdff-7132-44dc-a631-dc9576ad8b39", category.CategoryUuid);
        Assert.AreEqual("文房具", category.Name);
        _testContext?.WriteLine(category.ToString());
    }

    [TestMethod("存在しない商品カテゴリIdの場合はnullを返す")]
    public async Task SelectByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        var category = await _productCategoryRepository
           .SelectByIdAsync("2f4d3e51-6f6b-11f0-954a-00155d1bd30a");
        // nullであることを検証する
        Assert.IsNull(category);
    }
}