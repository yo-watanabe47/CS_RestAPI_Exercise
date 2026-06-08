using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Usecases.Users.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.Controllers;
using RestAPI_Exercise.Presentation.ViewModels;

namespace RestAPI_Exercise.Presentation.Tests.Controllers;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class RegisterUserControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[ユーザーを登録する]を実現するインターフェイス
    private IRegisterUserUsecase? _usecase;
    // RegisterUserViewModelからドメインオブジェクト:Userへ変換するアダプタ
    private RegisterUserViewModelAdapter? _adapter;
    // テストターゲット
    private RegisterUserController? _controller;
    // UserRepository
    private IUserRepository? _repository;

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
        // [ユーザーを登録する]を実現インターフェイスを取得する
        _usecase = _scope.ServiceProvider.GetRequiredService<IRegisterUserUsecase>();
        // RegisterUserViewModelからドメインオブジェクト:Userへ変換するアダプタを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<RegisterUserViewModelAdapter>();
        // テストターゲットを生成する
        _controller = new RegisterUserController(_usecase, _adapter);
        // UserRepositoryを取得する
        _repository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
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

    [TestMethod("ユーザー重複チェック:usernameとemailが両方nullの場合、BadRequest(400)が返される")]
    public async Task CheckDuplicate_ShouldReturnBadRequest_WhenNoParams()
    {
        var response = await _controller!.CheckDuplicate(null, null);
        // IActionResultをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        // エラーメッセージを取得する
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        // エラーメッセージを検証する
        Assert.AreEqual("usernameまたはemailのいずれかを指定してください。", msg);
    }

    [TestMethod("ユーザー重複チェック:存在しないユーザーの場合、OK(200)とexists=falseが返される")]
    public async Task CheckDuplicate_ShouldReturnOk_WhenNotExists()
    {
        var response = await _controller!.CheckDuplicate("notexists", "notexists@example.com");
        // IActionResultをOkObjectResultに変換する
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスを取得する
        var val = ok!.Value!;
        // existsを取得する
        var exists = (bool)(val.GetType().GetProperty("exists")!.GetValue(val)!);
        // existsがfalseであることを検証する
        Assert.IsFalse(exists);
    }

    [TestMethod("ユーザー登録:バリデーションエラーの場合、BadRequest(400)とエラーが返される")]
    public async Task Register_ShouldReturnBadRequest_WhenModelInvalid()
    {
        // エラーメッセージを設定する
        _controller!.ModelState.AddModelError("Username", "ユーザー名は必須です。");
        // RegisterUserViewModelを生成する
        var viewModel = new RegisterUserViewModel
        {
            Username = "",
            Email = "taro@example.com",
            Password = "pass"
        };
        // ユーザーを登録する
        var response = await _controller.Register(viewModel);
        // responseをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスを取り出す
        var val = bad!.Value!;
        // エラーコードを取得する
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // メッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Nameプロパティの値がエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Username"));
        // エラーメッセージを検証する
        CollectionAssert.Contains(details["Username"], "ユーザー名は必須です。");
    }

    [TestMethod("ユーザー登録:既に存在するユーザーの場合、Conflict(409)とエラーが返される")]
    public async Task Register_ShouldReturnConflict_WhenAlreadyExists()
    {
        // RegisterUserViewModelを生成する
        var viewModel = new RegisterUserViewModel
        {
            Username = "jiro",
            Email = "jiro@example.com",
            Password = "P@ssw0rd123!"
        };
        // ユーザーを登録する
        var first = await _controller!.Register(viewModel);
        // firstをCreatedResultに変換する
        var created = first as CreatedResult;
        // nullでないことを検証する
        Assert.IsNotNull(created);

        // 同じユーザーを登録する
        var second = await _controller.Register(viewModel);
        // secondをConflictObjectResultに変換する
        var conflict = second as ConflictObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(conflict);
        // レスポンスを取り出す
        var val = conflict!.Value!;
        // エラーコードを取得する
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        // エラーメッセージを取得する
        var message = val.GetType().GetProperty("message")?.GetValue(val) as string;
        // エラーコードを検証する
        Assert.AreEqual("USER_ALREADY_EXISTS", code);
        // メッセージを検証する
        Assert.AreEqual(
            "ユーザー名:jiroまたは、メールアドレス:jiro@example.comのユーザーは既に存在します。",
             message);
        // クリーニング
        var user = await _repository!.SelectByEmailAsync("jiro@example.com");
        await _repository!.DeleteByUserIdAsync(user!.UserUuid);
    }

    [TestMethod("ユーザー登録:正常に登録できる場合、Created(201)とユーザー情報が返される")]
    public async Task Register_ShouldReturnCreated_WhenSuccess()
    {
        // ViewModelを作成する
        var email = $"taro+{Guid.NewGuid():N}@example.com";
        var viewModel = new RegisterUserViewModel
        {
            Username = "taro",
            Email = email,
            Password = "P@ssw0rd123!"
        };
        // ユーザーを登録する
        var response = await _controller!.Register(viewModel);
        // responseをCreatedResultに変換する
        var created = response as CreatedResult;
        // nullでないことを検証する
        Assert.IsNotNull(created);
        // ステータスがCreated(201)であることを検証する
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);

        // クリーニング
        var user = await _repository!.SelectByEmailAsync(email);
        await _repository!.DeleteByUserIdAsync(user!.UserUuid);
    }
}