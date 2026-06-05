using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.Controllers;

namespace RestAPI_Exercise.Presentation.Tests.Controllers;
/// <summary>
/// ユースケース:[商品をキーワード検索する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class SearchProductByKeywordControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[商品をキーワード検索する]を実現するインターフェイス
    private ISearchProductByKeywordUsecase? _usecase;
    // テストターゲット
    private SearchProductByKeywordController? _controller;

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

    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // [商品をキーワード検索する]を実現インターフェイスを取得する
        _usecase = _scope.ServiceProvider.GetRequiredService<ISearchProductByKeywordUsecase>();
        // テストターゲットを生成する
        // services.AddControllers()では、Controllerそのものは登録されないため  
        _controller = new SearchProductByKeywordController(_usecase!);
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

    [TestMethod("keywordが未入力の場合、BadRequest(400)が返される")]
    public async Task Search_ShouldReturnBadRequest_WhenKeywordIsEmpty()
    {
        var result = await _controller!.Search("  ");
        // result(IActionResult)をBadRequestObjectResultに変換する
        var bad = result as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスステータスを検証する
        Assert.AreEqual(StatusCodes.Status400BadRequest, bad!.StatusCode);
        // レスポンスボディを取得する
        var val = bad.Value!;
        // コードを取り出す
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        // メッセージを取り出す
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        // コードを検証する
        Assert.AreEqual("INVALID_KEYWORD", code);
        // メッセージを検証する
        Assert.AreEqual("検索キーワードを入力してください。", msg);
    }

    [TestMethod("存在するキーワードの場合、ステータス200と商品リストを返す（蛍光 → 4件）")]
    public async Task Search_ShouldReturnOkWithProducts_WhenKeywordExists()
    {
        var result = await _controller!.Search("蛍光");
        // result(IActionResult)をOkObjectResultに変換する
        var ok = result as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスステータスを検証する
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        // レスポンスボディを取り出す
        var products = ok.Value as List<Product>;
        // nullでないことを検証する
        Assert.IsNotNull(products);
        // 件数が4であることを検証する
        Assert.AreEqual(4, products!.Count);
        // 取得結果を表示する
        foreach (var product in products)
        {
            _testContext?.WriteLine(product.ToString());
        }
    }

    [TestMethod("存在しないキーワードの場合、ステータス200と空配列を返す")]
    public async Task Search_ShouldReturnOkWithEmptyList_WhenNoMatches()
    {
        var result = await _controller!.Search("ゴム");
        // result(IActionResult)をOkObjectResultに変換する
        var ok = result as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスステータスを検証する
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        // レスポンスボディを取得する
        var products = ok.Value as List<Product>;
        // nullでないことを検証する
        Assert.IsNotNull(products);
        // 件数が0であることを検証する
        Assert.AreEqual(0, products!.Count);
    }

    [TestMethod("前後空白がある場合、トリミングされて検索される（\"  蛍光  \" → 4件）")]
    public async Task Search_ShouldTrimKeyword_BeforeUsecase()
    {
        var result = await _controller!.Search("  蛍光  ");
        // result(IActionResult)をOkObjectResultに変換する
        var ok = result as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスステータスを検証する
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        // レポンスボディを取得する
        var products = ok.Value as List<Product>;
        // nullでないことを検証する
        Assert.IsNotNull(products);
        // 件数が4であることを検証する
        Assert.AreEqual(4, products!.Count);
        // 取得結果を表示する
        foreach (var product in products)
        {
            _testContext?.WriteLine(product.ToString());
        }
    }
}