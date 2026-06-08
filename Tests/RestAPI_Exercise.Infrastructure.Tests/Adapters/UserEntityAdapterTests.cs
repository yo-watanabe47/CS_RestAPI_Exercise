using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Entities;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Adapters;
/// <summary>
/// ドメインオブジェクト:UserとUserEntityの相互変換クラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class UserEntityAdapterTests
{
    // テストターゲット
    private UserEntityAdapter _adapter = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        // アプリケーション構成を読み込む
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false).Build();
        // サービスプロバイダ(DIコンテナ)を生成する
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスのクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider?.Dispose();
    }

    /// <summary>
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider!.CreateScope();
        _adapter = _scope.ServiceProvider.GetRequiredService<UserEntityAdapter>();
    }

    /// <summary>
    /// テストの後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope?.Dispose();
    }

    [TestMethod("UserからUserEntityに変換できる")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly()
    {
        // データを用意する
        var uuid = Guid.NewGuid().ToString();
        var domain = new User(uuid, "Taro", "taro@example.com", "hashedpwd");
        // UserからUserEntityに変換する
        var entity = await _adapter.ConvertAsync(domain);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // ユーザーIdを検証する
        Assert.AreEqual(uuid, entity.UserUuid);
        // ユーザー名を検証する
        Assert.AreEqual("Taro", entity.Username);
        // メールアドレスを検証する
        Assert.AreEqual("taro@example.com", entity.Email);
        // パスワードを検証する
        Assert.AreEqual("hashedpwd", entity.PasswordHash);
    }

    [TestMethod("nullを渡すとInternalExceptionをスローする")]
    public async Task ConvertAsync_Should_ThrowException_When_Null()
    {
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            _ = await _adapter.ConvertAsync(null!);
        });
        Assert.AreEqual("引数domainがnullです。", ex.Message);
    }

    [TestMethod("UserEntityからUserを復元できる")]
    public async Task RestoreAsync_Should_MapPropertiesCorrectly()
    {
        // データを用意する
        var uuid = Guid.NewGuid().ToString();
        var entity = new UserEntity
        {
            UserUuid = uuid,
            Username = "Hanako",
            Email = "hanako@example.com",
            PasswordHash = "securehash"
        };

        // UserEntityからUserを復元する
        var domain = await _adapter.RestoreAsync(entity);

        // nullでないことを検証する
        Assert.IsNotNull(domain);
        // ユーザーIdを検証する
        Assert.AreEqual(uuid, domain.UserUuid);
        // ユーザー名を検証する
        Assert.AreEqual("Hanako", domain.Username);
        // メールアドレスを検証する
        Assert.AreEqual("hanako@example.com", domain.Email);
        // パスワードを検証する
        Assert.AreEqual("securehash", domain.Password);
    }

    [TestMethod("nullを渡すとInternalExceptionをスローする")]
    public async Task RestoreAsync_Should_ThrowException_When_Null()
    {
        var ex = await Assert.ThrowsExactlyAsync<InternalException>(async () =>
        {
            _ = await _adapter.RestoreAsync(null!);
        });
        Assert.AreEqual("引数targetがnullです。", ex.Message);
    }
}