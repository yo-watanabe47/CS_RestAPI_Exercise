using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.Controllers;
using RestAPI_Exercise.Presentation.ViewModels;
namespace RestAPI_Exercise.Presentation.Tests.Controllers;
/// <summary>
/// ユースケース:[商品を変更する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class UpdateProductControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[新商品を登録する]を実現するインターフェイス
    private IUpdateProductUsecase? _usecase;
    // UpdateProductViewModelからドメインオブジェクト:Productへ変換するアダプタ
    private UpdateProductViewModelAdapter? _adapter;
    // テストターゲット
    private UpdateProductController? _controller;

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
        // [新商品を登録する]を実現インターフェイスを取得する
        _usecase = _scope.ServiceProvider.GetRequiredService<IUpdateProductUsecase>();
        // RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<UpdateProductViewModelAdapter>();
        // テストターゲットを生成する
        _controller = new UpdateProductController(_usecase, _adapter);
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

    [TestMethod("変更商品の取得:存在しない商品Idの場合、NotFound(404)とエラーが返される")]
    public async Task GetProductById_ShouldReturnNotFound_WhenMissing()
    {
        var id = Guid.NewGuid().ToString();
        var response = await _controller!.GetProductById(id);
        // responseをNotFoundObjectResultに変換する
        var notfound = response as NotFoundObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("PRODUCT_NOT_FOUND", code);
        // エラーメッセージを検証する
        Assert.AreEqual($"商品Id:{id}の商品は存在しません。", msg);
    }

    [TestMethod("変更商品の取得:存在する商品Idの場合、OK(200)と商品が返される")]
    public async Task GetProductById_ShouldReturnOk_WhenFound()
    {
        var id = "e4850253-f363-4e79-8110-7335e4af45be";
        var response = await _controller!.GetProductById(id);
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // リクエストボディ:商品を取得する
        var product = ok!.Value as Product;
        // nullでないことを検証する
        Assert.IsNotNull(product);
        // 商品Idを検証する
        Assert.AreEqual(id, product!.ProductUuid);
        // 商品名を検証する
        Assert.AreEqual("鉛筆(黒)", product!.Name);
        // 単価を検証する
        Assert.AreEqual(100, product!.Price);
        // 在庫数を検証する
        Assert.AreEqual(100, product.Stock!.Stock);
    }

    [TestMethod("商品名の有無チェック:未入力の場合、BadRequest(400)とエラーが返される")]
    public async Task ValidateProduct_ShouldReturnBadRequest_WhenEmpty()
    {
        var response = await _controller!.ValidateProduct("  ");
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("INVALID_PRODUCT_NAME", code);
        // メッセージを検証する
        Assert.AreEqual("商品名は必須です。", msg);
    }
    [TestMethod("商品名の有無チェック:存在する商品名の場合、Conflict(409)とエラーが返される")]
    public async Task ValidateProduct_ShouldReturnConflict_WhenExists()
    {
        var response = await _controller!.ValidateProduct("油性ボールペン(赤)");
        var conflict = response as ConflictObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(conflict);
        var val = conflict!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        // メッセージを検証する
        Assert.AreEqual("商品名:油性ボールペン(赤)は既に存在します。", msg);
    }

    [TestMethod("商品変更:バリデーションエラーの場合、BadRequest(400)とエラーが返される)")]
    public async Task Updated_ShouldReturnBadRequest_WhenModelInvalid()
    {
        _controller!.ModelState.AddModelError("Name", "商品名は必須です。");
        var vm = new UpdateProductViewModel
        {
            ProductId = Guid.NewGuid().ToString(),
            Name = "",
            Price = 100,
            Stock = 10,
        };
        var res = await _controller.Updated(vm);
        var bad = res as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("VALIDATION_ERROR", code);
        // バリデーションメッセージを取得する
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // エラーメッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Nameプロパティのエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Name"));
    }

    [TestMethod("商品変更:存在する商品名で変更した場合、Conflict(409)とエラーが返される")]
    public async Task Updated_ShouldReturnConflict_WhenRenameToExistingName()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = "8f81a72a-58ef-422b-b472-d982e8665292",
            Name = "水性ボールペン(赤)",
            Price = 100,
            Stock = 10,
        };
        var res = await _controller!.Updated(viewModel);
        var conflict = res as ConflictObjectResult;
        Assert.IsNotNull(conflict);
        var val = conflict!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        Assert.AreEqual("商品名:水性ボールペン(赤)は既に存在します。", msg);
    }

    [TestMethod("商品変更:業務ルール違反の場合、BadRequest(400)とエラーが返される")]
    public async Task Updated_ShouldReturnBadRequest_WhenDomainViolation()
    {
        var viewModel = new UpdateProductViewModel
        {
            ProductId = "8f81a72a-58ef-422b-b472-d982e8665292",
            Name = "水性ボールペン(緑)",
            Price = -1, // 業務ルール違反
            Stock = 10,
        };
        var response = await _controller!.Updated(viewModel);
        var bad = response as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        Assert.AreEqual("DOMAIN_RULE_VIOLATION", code);
        Assert.AreEqual("価格は0円以上である必要があります。", msg);
    }

    [TestMethod("商品変更:矛盾のない値の場合、Ok(200)と変更された商品が返される")]
    public async Task Updated_ShouldReturnOk_WhenSuccess()
    {
        var originViewModel = new UpdateProductViewModel
        {
            ProductId = "8f81a72a-58ef-422b-b472-d982e8665292",
            Name = "水性ボールペン(赤)",
            Price = 100,
            Stock = 10,
        };
        var updateViewModel = new UpdateProductViewModel
        {
            ProductId = "8f81a72a-58ef-422b-b472-d982e8665292",
            Name = "水性ボールペン(緑)",
            Price = 150,
            Stock = 30,
        };

        var response = await _controller!.Updated(updateViewModel);
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // リクエストボディから商品を取得する
        var product = ok!.Value as Product;
        // nullでないことを検証する
        Assert.IsNotNull(product);
        // 商品Idを検証する
        Assert.AreEqual(updateViewModel.ProductId, product!.ProductUuid);
        // 単価を検証する
        Assert.AreEqual(updateViewModel.Price, product.Price);
        // 在庫数を検証する
        Assert.AreEqual(updateViewModel.Stock, product.Stock!.Stock);
        // 商品名を検証する
        Assert.AreEqual(updateViewModel.Name, product.Name);
        // 変更データを復元する
        await _controller!.Updated(originViewModel);
    }
}