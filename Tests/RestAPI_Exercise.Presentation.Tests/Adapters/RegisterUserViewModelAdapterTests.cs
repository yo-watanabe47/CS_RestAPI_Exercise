using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.Configs;
using RestAPI_Exercise.Presentation.ViewModels;

namespace RestAPI_Exercise.Presentation.Tests.Adapters;
/// <summary>
/// RegisterUserViewModelAdapter のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class RegisterUserViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private RegisterUserViewModelAdapter? _adapter;

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
        // テストターゲットを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<RegisterUserViewModelAdapter>();
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

    [TestMethod("ViewModelからUserへ復元でき、UUIDが生成される")]
    public async Task RestoreAsync_ShouldMapVmToDomain_AndGenerateUuid()
    {
        // ViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Email = $"taro+{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd1"
        };
        // ViewModelからUserを復元する
        var user = await _adapter!.RestoreAsync(vm);
        // ユーザー名を検証する
        Assert.AreEqual(vm.Username, user.Username);
        // メールアドレスを検証する
        Assert.AreEqual(vm.Email, user.Email);
        // パスワードを検証する
        Assert.AreEqual(vm.Password, user.Password);
        // UUIDが生成されたことを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(user.UserUuid));
        Assert.IsTrue(Guid.TryParse(user.UserUuid, out _));
    }

    [TestMethod("不正なユーザー名（空、長すぎ)の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenUsernameInvalid()
    {
        // ユーザー名が空のViewModel
        var vmEmpty = new RegisterUserViewModel
        {
            Username = " ",
            Email = $"u+{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd1"
        };
        // DomanExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmEmpty));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザー名は必須です。", ex.Message);

        // ユーザー名が31文字のViewModelを生成する
        var vmLong = new RegisterUserViewModel
        {
            Username = new string('x', 31),
            Email = $"u+{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd1"
        };
        ex = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmLong));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザー名は30文字以内で指定してください。", ex.Message);
    }

    [TestMethod("不正なメール形式や長すぎる場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenEmailInvalid()
    {
        // 不正なメール形式のViewModelを生成する
        var vmBadFormat = new RegisterUserViewModel
        {
            Username = "taro",
            Email = "not-an-email",
            Password = "P@ssw0rd1"
        };
        // DomainExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmBadFormat));
        // エラーメッセージを検証する
        Assert.AreEqual("メールアドレスの形式が不正です。", ex.Message);

        // メールアドレスが101文字のViewModelを生成する
        var longLocal = new string('a', 101 - "@x.io".Length);
        var vmTooLong = new RegisterUserViewModel
        {
            Username = "taro",
            Email = $"{longLocal}@x.io",
            Password = "P@ssw0rd1"
        };
        // DomainExceptionがスローされることを検証する
        ex = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmTooLong));
        // エラーメッセージを検証する
        Assert.AreEqual("メールアドレスは100文字以内で指定してください。", ex.Message);
    }

    [TestMethod("パスワードが空の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenPasswordEmpty()
    {
        // パスワードが空のViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Email = $"taro+{Guid.NewGuid():N}@example.com",
            Password = " "
        };
        // DomainExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExactlyAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vm));
        // エラーメッセージを検証する
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }
}