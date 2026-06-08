using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Security;
using RestAPI_Exercise.Application.Usecases.Authenticate.Interfaces;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.Controllers;
using RestAPI_Exercise.Presentation.ViewModels;

namespace RestAPI_Exercise.Presentation.Tests.Controllers;
/// <summary>
/// ユースケース:[ログイン/ログアウト] を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class AuthenticateControllerTests
{
    // MSTestログ出力
    private static TestContext? _testContext;
    // DIコンテナ
    private static ServiceProvider? _provider;
    // スコープ
    private IServiceScope? _scope;
    // ユースケース:[ログインする]を実現するインターフェイス
    private IAuthenticateUserUsecase? _usecase;
    // JWTの発行・検証インターフェイス
    private IJwtTokenProvider? _tokenProvider;
    // ユーザーリポジトリインターフェイス
    private IUserRepository? _userRepository;
    // パスワードのハッシュ化と検証機能を提供するインターフェイス
    private IPasswordHashingService? _hashing;
    // テストターゲット
    private AuthenticateController? _controller;

    /// <summary>クラス初期化</summary>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _testContext = context;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>クラスクリーンアップ</summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider?.Dispose();
    }

    /// <summary>テスト前処理</summary>
    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider!.CreateScope();

        _usecase = _scope.ServiceProvider.GetRequiredService<IAuthenticateUserUsecase>();
        _tokenProvider = _scope.ServiceProvider.GetRequiredService<IJwtTokenProvider>();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _hashing = _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

        _controller = new AuthenticateController(_usecase!, _tokenProvider!);
    }

    /// <summary>テスト後処理</summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope?.Dispose();
    }

    [TestMethod("存在しないユーザーの場合、Unauthorized(401)が返される")]
    public async Task Login_ShouldReturnUnauthorized_WhenAuthFails()
    {
        // 認証データを用意する
        var viewModel = new LoginViewModel
        {
            UsernameOrEmail = "no_such_user@example.com",
            Password = "wrong"
        };
        // 認証する
        var response = await _controller!.Login(viewModel);
        // responseをUnauthorizedObjectResultに変換する
        var unauthorized = response as UnauthorizedObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(unauthorized);
    }

    [TestMethod("存在するメールアドレスとパスワードの場合、Ok(200)とJWTトークンが返される")]
    public async Task Login_ShouldReturnOk_WithToken_WhenSuccess()
    {
        // テストデータを用意する
        var email = $"login+{Guid.NewGuid():N}@example.com";
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 12);
        var rawPassword = "P@ssw0rd123!";
        var hashed = _hashing!.Hash(rawPassword);
        var user = new User(username, email, hashed);
        // テストデータを登録する
        await _userRepository!.CreateAsync(user);
        // ログインデータを用意する
        var viewModel = new LoginViewModel
        {
            UsernameOrEmail = email,
            Password = rawPassword
        };
        try
        {
            // ログイン認証する
            var response = await _controller!.Login(viewModel);
            // responseをOkObjectResultに変換する
            var ok = response as OkObjectResult;
            // nullでないことを検証する
            Assert.IsNotNull(ok);
            // レスポンスボディを取得する
            var body = ok!.Value!;
            // プロパティを取得する
            var tokenProp = body.GetType().GetProperty("Token");
            // トークンを取得する
            var token = tokenProp?.GetValue(body) as string;
            // nullや空白でないことを検証する
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        }
        finally
        {
            // クリーニング
            // メールアドレスで登録ユーザーを取得する
            var u = await _userRepository!.SelectByEmailAsync(email);
            // 登録ユーザーを削除する   
            await _userRepository.DeleteByUserIdAsync(u.UserUuid);
        }
    }
    [TestMethod("ログアウトすると、NoContent(204)が返される")]
    public void Logout_ShouldReturnNoContent_WhenAuthenticated()
    {
        // 認証済みユーザーを偽装してControllerContextに設定する
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.Name, "tester")
        };
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        // ログアウト用コントローラを用意する
        _controller!.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        // ログアウトする
        var response = _controller.Logout();
        // responseをNoContentResultに変換する
        var noContent = response as NoContentResult;
        // nullでないことを検証する
        Assert.IsNotNull(noContent);
        // ステータスを検証する
        Assert.AreEqual(StatusCodes.Status204NoContent, noContent!.StatusCode);
    }
}