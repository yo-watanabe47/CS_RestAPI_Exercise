using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Infrastructure.Contexts;
using RestAPI_Exercise.Presentation.Configs;
namespace RestAPI_Exercise.Infrastructure.Tests.Repositories;
/// <summary>
/// ドメインオブジェクト:UserのCRUD操作インターフェイス実装の単体テストドライバ
/// </summary>
[DoNotParallelize]
[TestClass]
[TestCategory("Repositories")]
public class UserRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // App用DbContext
    private AppDbContext? _dbContext;
    // テストターゲット
    private IUserRepository _userRepository = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // MSTestテスト用ログ出力ハンドルを設定する
        _testContext = context;

        // アプリケーション構成を読み込む
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
        _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        // DbContextを取得する
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// テストの後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope!.Dispose();
    }

    [TestMethod("ユーザーを永続化できる")]
    public async Task CreateAsync_ShouldPersistUser()
    {
        // Arrange
        var user = new User("taro_user", "taro@example.com", "hashedpwd"); // UUIDは自動生成

        // MySQLのExecutionStrategy配下で手動Txを1単位として実行
        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                // Act
                await _userRepository.CreateAsync(user);

                // Assert: メールで取得して一致を検証
                var persisted = await _userRepository.SelectByEmailAsync("taro@example.com");
                Assert.IsNotNull(persisted);
                Assert.AreEqual(user.UserUuid, persisted!.UserUuid);
                Assert.AreEqual("taro_user", persisted.Username);
                Assert.AreEqual("taro@example.com", persisted.Email);
                Assert.AreEqual("hashedpwd", persisted.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }


    [TestMethod("ユーザー名またはメールが存在するとtrueが返る")]
    public async Task ExistsByUsernameOrEmailAsync_WhenExists_ShouldReturnTrue()
    {
        var user = new User("hanako_user", "hanako@example.com", "pwdhash");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                // 事前に作成
                await _userRepository.CreateAsync(user);
                // ユーザー名でヒット
                var byName = await _userRepository.ExistsByUsernameOrEmailAsync("hanako_user", "no-hit@example.com");
                Assert.IsTrue(byName);
                // メールでヒット
                var byEmail = await _userRepository.ExistsByUsernameOrEmailAsync("no-hit", "hanako@example.com");
                Assert.IsTrue(byEmail);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザー名またはメールが存在しないとfalseが返る")]
    public async Task ExistsByUsernameOrEmailAsync_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _userRepository.ExistsByUsernameOrEmailAsync("nobody", "nobody@example.com");
        Assert.IsFalse(result);
    }

    [TestMethod("ユーザー名またはメールからユーザーを取得できる（ユーザー名）")]
    public async Task SelectByUsernameOrEmailAsync_ByUsername_ShouldReturnUser()
    {
        var u = new User("jiro_user", "jiro@example.com", "hash");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                await _userRepository.CreateAsync(u);

                var result = await _userRepository.SelectByUsernameOrEmailAsync("jiro_user");
                Assert.IsNotNull(result);
                Assert.AreEqual(u.UserUuid, result!.UserUuid);
                Assert.AreEqual("jiro_user", result.Username);
                Assert.AreEqual("jiro@example.com", result.Email);
                Assert.AreEqual("hash", result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザー名またはメールからユーザーを取得できる（メール）")]
    public async Task SelectByUsernameOrEmailAsync_ByEmail_ShouldReturnUser()
    {
        var u = new User("sabo_user", "sabo@example.com", "hash2");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                await _userRepository.CreateAsync(u);

                var result = await _userRepository.SelectByUsernameOrEmailAsync("sabo@example.com");
                Assert.IsNotNull(result);
                Assert.AreEqual(u.UserUuid, result!.UserUuid);
                Assert.AreEqual("sabo_user", result.Username);
                Assert.AreEqual("sabo@example.com", result.Email);
                Assert.AreEqual("hash2", result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザー名またはメールに一致しない場合はnullが返る")]
    public async Task SelectByUsernameOrEmailAsync_WhenNoMatch_ShouldReturnNull()
    {
        var result = await _userRepository
            .SelectByUsernameOrEmailAsync("no-hit@example.com");
        Assert.IsNull(result);
    }


    [TestMethod("メールからユーザーを取得できる")]
    public async Task SelectByEmailAsync_WhenExists_ShouldReturnUser()
    {
        var u = new User("mike_user", "mike@example.com", "hash3");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                await _userRepository.CreateAsync(u);

                var result = await _userRepository.SelectByEmailAsync("mike@example.com");
                Assert.IsNotNull(result);
                Assert.AreEqual(u.UserUuid, result!.UserUuid);
                Assert.AreEqual("mike_user", result.Username);
                Assert.AreEqual("mike@example.com", result.Email);
                Assert.AreEqual("hash3", result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("メールに一致しない場合はnullが返る")]
    public async Task SelectByEmailAsync_WhenNotExists_ShouldReturnNull()
    {
        var result = await _userRepository.SelectByEmailAsync("nobody@example.com");
        Assert.IsNull(result);
    }

    [TestMethod("ユーザーIdでユーザーを取得できる")]
    public async Task SelectByIdAsync_WhenExists_ShouldReturnUser()
    {
        var u = new User("nick_user", "nick@example.com", "hash4");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                await _userRepository.CreateAsync(u);

                var result = await _userRepository.SelectByIdAsync(u.UserUuid);
                Assert.IsNotNull(result);
                Assert.AreEqual(u.UserUuid, result!.UserUuid);
                Assert.AreEqual("nick_user", result.Username);
                Assert.AreEqual("nick@example.com", result.Email);
                Assert.AreEqual("hash4", result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                tx.Dispose();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }
    
    [TestMethod("ユーザーIdに一致しない場合はnullが返る")]
    public async Task SelectByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        var result = await _userRepository.SelectByIdAsync(Guid.NewGuid().ToString());
        Assert.IsNull(result);
    }
}