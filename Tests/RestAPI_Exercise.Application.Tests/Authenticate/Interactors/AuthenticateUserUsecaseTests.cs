using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Security;
using RestAPI_Exercise.Application.Usecases.Authenticate.Interfaces;
using RestAPI_Exercise.Presentation.Configs;

namespace RestAPI_Exercise.Application.Tests.Usecase.Authenticate.Interactors;
/// <summary>
/// ユースケース:[ログインする]を実現するインターフェイス実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Usecase/Authenticate/Interactors")]
public class AuthenticateUserUsecaseTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private IAuthenticateUserUsecase? _usecase;
    // UserのCRUD操作リポジトリ
    private IUserRepository? _repository;
    // パスワードのハッシュ化と検証サービス
    private IPasswordHashingService? _service;

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
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _usecase =
        _scope.ServiceProvider.GetRequiredService<IAuthenticateUserUsecase>();
        _repository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _service = _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
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

    [TestMethod("メールアドレスと正しいパスワードでUserが返される")]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenEmailAndPasswordAreCorrect()
    {
        // ユーザーを生成する
        var password = _service!.Hash("P@ssw0rd123!");
        var email = $"taro+{Guid.NewGuid():N}@example.com";
        var user = new User("taro", email, password);
        try
        {
            // ユーザーを登録する
            await _repository!.CreateAsync(user);
            // 認証処理をする
            var authed = await _usecase!.AuthenticateAsync(email, "P@ssw0rd123!");
            // nullでないことを検証する
            Assert.IsNotNull(authed);
            // ユーザーIdを検証する
            Assert.AreEqual(user.UserUuid, authed.UserUuid);
            // ユーザー名を検証する
            Assert.AreEqual("taro", authed.Username);
            // メールアドレスを検証する
            Assert.AreEqual(email, authed.Email);
        }
        finally
        {
            // クリーニング:登録ユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }

    [TestMethod("ユーザー名と正しいパスワードでUserが返される")]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenUsernameAndPasswordAreCorrect()
    {
        // ユーザーを生成する
        var password = _service!.Hash("P@ssw0rd123!");
        var email = $"taro+{Guid.NewGuid():N}@example.com";
        var user = new User("jiro", email, password);

        try
        {
            // ユーザーを登録する
            await _repository!.CreateAsync(user);
            // 認証処理をする
            var authed = await _usecase!.AuthenticateAsync("jiro", "P@ssw0rd123!");
            // nullでないことを検証する
            Assert.IsNotNull(authed);
            // ユーザーIdを検証する
            Assert.AreEqual(user.UserUuid, authed.UserUuid);
            // ユーザー名を検証する
            Assert.AreEqual("jiro", authed.Username);
            // メールアドレスを検証する
            Assert.AreEqual(email, authed.Email);
        }
        finally
        {
            // クリーニング:登録ユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }

    [TestMethod("ユーザーが存在しない場合、AuthenticationExceptionがスローされる")]
    public async Task AuthenticateAsync_ShouldThrow_WhenUserNotFound()
    {
        // AuthenticationExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExactlyAsync<AuthenticationException>(async () =>
        {
            await _usecase!.AuthenticateAsync("not-found-user@example.com", "any");
        });
        // メッセージを検証する
        Assert.AreEqual("ユーザーが存在しません。", ex.Message);
    }

    [TestMethod("パスワードが不一致の場合、AuthenticationExceptionがスローされる")]
    public async Task AuthenticateAsync_ShouldThrow_WhenPasswordMismatch()
    {
        var email = $"hanako+{Guid.NewGuid():N}@example.com";
        var user  = new User("hanako", email, _service!.Hash("CorrectP@ss!"));

        try
        {
            // ユーザーを登録する
            await _repository!.CreateAsync(user);
            // AuthenticationExceptionがスローされることを検証する
            Exception ex = await Assert.ThrowsExactlyAsync<AuthenticationException>(async () =>
            {
                await _usecase!.AuthenticateAsync(email, "WrongPassword");
            });
            // メッセージを検証する
            Assert.AreEqual("パスワードが一致しません。", ex.Message);
        }
        finally
        {
            // クリーニング:登録ユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }
}