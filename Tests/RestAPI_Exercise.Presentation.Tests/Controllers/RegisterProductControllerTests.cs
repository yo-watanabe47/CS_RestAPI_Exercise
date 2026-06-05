using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.Controllers;
using RestAPI_Exercise.Presentation.ViewModels;
namespace RestAPI_Exercise.Presentation.Tests.Controllers;
/// <summary>
/// ユースケース:[新商品を登録する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class RegisterProductControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[新商品を登録する]を実現するインターフェイス
    private IRegisterProductUsecase? _usecase;
    // RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ
    private RegisterProductViewModelAdapter? _adapter;
    // テストターゲット
    private RegisterProductController? _controller;
    // ProductRepository
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
    /// テストメソッド実行の前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // [新商品を登録する]を実現インターフェイスを取得する
        _usecase = _scope.ServiceProvider.GetRequiredService<IRegisterProductUsecase>();
        // RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<RegisterProductViewModelAdapter>();
        // テストターゲットを生成する
        _controller = new RegisterProductController(_usecase, _adapter);
        // ProductRepositoryを取得する
        _repository = _scope.ServiceProvider.GetRequiredService<IProductRepository>();
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

    [TestMethod("商品カテゴリ一覧の取得:OK(200)とList<ProductCategory>を返す")]
    public async Task GetCategories_ShouldReturnOk()
    {
        var result = await _controller!.GetCategories();
        // IActionResultをOkObjectResultに変換する
        var ok = result as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // ステータスOK(200)であることを検証する
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        // レスポンスボディを取得する
        var categories = ok.Value as List<ProductCategory>;
        // nullでないことを検証する
        Assert.IsNotNull(categories);
        // 3件であることを検証する
        Assert.AreEqual(3, categories.Count);
        foreach (var category in categories)
        {
            _testContext!.WriteLine(category.ToString());
        }
    }

    [TestMethod("Idに一致する商品カテゴリの取得:存在する商品カテゴリIdの場合、Ok(200)と該当する商品カテゴリが返される   ")]
    public async Task GetCategoryById_ShouldWork_ForFound()
    {
        var response = await _controller!
            .GetCategoryById("bab15a9d-fb32-44ff-9dde-dc7fa32237e0");
        // レスポンスがOkObjectResultであることを検証する
        Assert.IsInstanceOfType(response, typeof(OkObjectResult));
        // レスポンスをOkObjectResultに変換する
        var okObj = response as OkObjectResult;
        // レスポンスボディを取得する
        var category = okObj!.Value as ProductCategory;
        // nullでないことを検証する
        Assert.IsNotNull(category);
        // 商品カテゴリIdを検証する
        Assert.AreEqual("bab15a9d-fb32-44ff-9dde-dc7fa32237e0", category!.CategoryUuid);
        Assert.AreEqual("雑貨", category!.Name);
    }

    [TestMethod("Idに一致する商品カテゴリの取得:存在しない商品カテゴリIdの場合、NotFiund(404)とエラーが返される")]
    public async Task GetCategoryById_ShouldWork_ForNotFound()
    {
        var response = await _controller!
            .GetCategoryById("2f5016b6-6f6b-11f0-954a-00155d1bd10a");
        // レスポンスをNotFoundObjectResultに変換する
        var notfound = response as NotFoundObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        // エラーコードを検証する
        Assert.AreEqual("CATEGORY_NOT_FOUND", code);
        // エラーメッセージを検証する
        Assert.AreEqual("商品カテゴリId:2f5016b6-6f6b-11f0-954a-00155d1bd10aの商品カテゴリは存在しません。"
            , msg);
    }

    [TestMethod("商品名有無チェック:商品名が未入力の場合、BadRequest(400)とエラーが返される")]
    public async Task ValidateProduct_ShouldReturnBadRequest_WhenNameEmpty()
    {
        var response = await _controller!.ValidateProduct("  ");
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("INVALID_PRODUCT_NAME", code);
        Assert.AreEqual("商品名は必須です。", msg);
    }

    [TestMethod("商品名有無チェック:存在する商品名の場合、Conflict(409)とエラーが返される")]
    public async Task ValidateProduct_ShouldReturnConflict_WhenExists()
    {
        var response = await _controller!.ValidateProduct("水性ボールペン(赤)");
        // レスポンスをConflictObjectResultに変換する
        var conflict = response as ConflictObjectResult;
        // レスポンスボディを取得する
        var val = conflict!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        Assert.AreEqual("商品名:水性ボールペン(赤)は既に存在します。", msg);
    }

    [TestMethod("商品名有無チェック:存在しない商品名の場合、OK(200)とfalseが返される")]
    public async Task ValidateProduct_ShouldReturnOk_WhenNotExists()
    {
        var response = await _controller!.ValidateProduct("消しゴム");
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスボディを取得する
        var val = ok!.Value!;
        var prop = val.GetType().GetProperty("exists");
        // nullでないことを検証する
        Assert.IsNotNull(prop);
        var exists = (bool)(prop!.GetValue(val)!);
        // falseであることを検証する
        Assert.IsFalse(exists);
    }

    [TestMethod("商品登録:バリデーションエラーの場合、BadRequest(400)とエラーが返される")]
    public async Task Register_ShouldReturnBadRequest_WhenModelInvalid()
    {
        // 自動バリデーション機能が利用できないので、予めエラーメッセージを設定する
        _controller!.ModelState.AddModelError("Name", "商品名は必須です。");
        var viewModel = new RegisterProductViewModel
        {
            Name = "",
            Price = 100,
            Stock = 10,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        // 商品登録を実行する
        var response = await _controller.Register(viewModel);
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // メッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Nameプロパティの値がエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Name"));
        // エラーメッセージを検証する
        CollectionAssert.Contains(details["Name"], "商品名は必須です。");
    }

    [TestMethod("商品登録:既に存在する商品名の場合、Conflict(Conflict)とエラーが返される")]
    public async Task Register_ShouldReturnConflict_WhenAlreadyExists()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "水性ボールペン(赤)",
            Price = 100,
            Stock = 10,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        var response = await _controller!.Register(viewModel);
        // レスポンスをConflictObjectResultに変換する
        var conflict = response as ConflictObjectResult;
        // レスポンスボディを取得する
        var val = conflict!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        Assert.AreEqual("商品名:水性ボールペン(赤)は既に存在します。", msg);
    }

    [TestMethod("商品登録:商品カテゴリが存在しない場合、NotFound(404)とエラーが返される")]
    public async Task Register_ShouldReturnNotFound_WhenCategoryMissing()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "水性ボールペン(赤)",
            Price = 100,
            Stock = 10,
            CategoryId = Guid.NewGuid().ToString(), // 存在しない商品カテゴリId
            CategoryName = "ダミー"
        };
        var res = await _controller!.Register(viewModel);
        var notfound = res as NotFoundObjectResult;
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("CATEGORY_NOT_FOUND", code);
        Assert.AreEqual($"商品カテゴリId:{viewModel.CategoryId}の商品カテゴリは存在しません。"
            , msg);
    }

    [TestMethod("商品登録:矛盾の無いデータの場合、Created(201)とLocationが返される")]
    public async Task Register_ShouldReturnCreated_WhenSuccess()
    {
        var viewModel = new RegisterProductViewModel
        {
            Name = "消しゴム",
            Price = 120,
            Stock = 10,
            CategoryId = "e3e5bdff-7132-44dc-a631-dc9576ad8b39",
            CategoryName = "文房具"
        };
        var response = await _controller!.Register(viewModel);
    
        var created = response as CreatedResult;
        // nullでないことを検証する
        Assert.IsNotNull(created);
        // ステータスがCreated(201)であることを検証する
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);
        // 登録されたデータを削除する
        var product = created.Value as Product; 
        Assert.IsNotNull(product);
        var id = product!.ProductUuid;            // 実際のプロパティ名に合わせる
        await _repository!.DeleteByIdAsync(id);
    }
}