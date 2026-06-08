using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Security;
using RestAPI_Exercise.Presentation.Configs;

namespace RestAPI_Exercise.Application.Tests.Security;
/// <summary>
///  PBKDF2アルゴリズムを利用
/// パスワードのハッシュ化と検証機能を提供するインターフェイス実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Security")]
public class PBKDF2PasswordHashingServiceTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
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
        _service =
        _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
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

    [TestMethod("パスワードをハッシュ化し、検証に成功する")]
    public void Hash_ShouldGenerateHash_AndVerifySuccess()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";
        // パスワードをハッシュ化する
        var hash = _service!.Hash(raw);
        // ハッシュが平文と異なることを検証する
        Assert.AreNotEqual(raw, hash);
        // 正しいパスワードで検証が成功することを検証
        Assert.IsTrue(_service!.Verify(hash, raw));
    }

    [TestMethod("同じパスワードをハッシュ化しても異なる値になる")]
    public void Hash_SamePassword_ShouldGenerateDifferentHashes()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";
        // パスワードを2回ハッシュ化する
        var h1 = _service!.Hash(raw);
        var h2 = _service!.Hash(raw);
        // ソルトにより毎回異なる値であることを検証する
        Assert.AreNotEqual(h1, h2);
        // それぞれのハッシュで検証が成功することを検証する
        Assert.IsTrue(_service!.Verify(h1, raw));
        Assert.IsTrue(_service!.Verify(h2, raw));
    }

    [TestMethod("間違ったパスワードの場合、検証に失敗する")]
    public void Verify_ShouldReturnFalse_WhenPasswordIsWrong()
    {
        // パスワードを用意する
        var raw = "P@ssw0rd123!";
        // ハッシュ化する
        var hash = _service!.Hash(raw);
        // 間違ったパスワードでは検証に失敗することを検証する
        Assert.IsFalse(_service!.Verify(hash, "wrong-pass"));
    }

    [TestMethod("古い形式のハッシュを検証すると再ハッシュ例外がスローされる")]
    public void Verify_ShouldThrowPasswordRehashNeededException_WhenOldHashFormat()
    {
        var raw = "P@ssw0rd123!";

        // IdentityV2のハッシュ生成機能を作成する
        var oldHasher = new PasswordHasher<User>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            })
        );
        // 古い形式のハッシュ生成機能でServiceを生成する
        var oldSvc = new PBKDF2PasswordHashingService(oldHasher);
        // 古い形式でパスワードをハッシュ化する
        var oldHash = oldSvc.Hash(raw);

        // PasswordRehashNeededException例外がスローされることを検証する
        Exception ex = Assert.ThrowsExactly<PasswordRehashNeededException>(() =>
        {
            _service!.Verify(oldHash, raw);
        });
        Assert.AreEqual("パスワードは認証されたが、再ハッシュが必要です。", ex.Message);
    }
}