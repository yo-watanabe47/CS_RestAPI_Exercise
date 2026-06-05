using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using RestAPI_Exercise.Infrastructure.Contexts;
using RestAPI_Exercise.Infrastructure.Shared;
namespace RestAPI_Exercise.Infrastructure.Tests.Shared;
/// <summary>
/// Unit of Workパターンを利用したトランザクション制御インターフェイス実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Shared")]
public class UnitOfWorkTests
{
    // DbContext継承クラスのモック
    private Mock<AppDbContext> _mockDbContext = null!;
    // DatabaseFacede(Databaseプロパティ)のモック
    private Mock<DatabaseFacade> _mockDatabase = null!;
    // トランザクション制御機能のモック
    private Mock<IDbContextTransaction> _mockTransaction = null!;
    // テストターゲット
    private UnitOfWork _unitOfWork = null!;

    /// <summary>
    /// 各テストの前に呼び出される初期化処理
    /// モックオブジェクトを作成してUnitOfWorkに注入する
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        // AppDbContext をモック化する
        _mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        // DbContext.Databaseから取得されるDatabaseFacadeをモック化する
        _mockDatabase = new Mock<DatabaseFacade>(_mockDbContext.Object);
        // BeginTransactionAsync()の戻り値として返されるIDbContextTransactionをモック化する
        _mockTransaction = new Mock<IDbContextTransaction>();

        // DbContext.Databaseプロパティにモックを設定する
        _mockDbContext.Setup(c => c.Database).Returns(_mockDatabase.Object);
        // BeginTransactionAsync()を呼ぶとモックのトランザクションを返すよう設定する
        _mockDatabase
            .Setup(d => d.BeginTransactionAsync(default))
            .ReturnsAsync(_mockTransaction.Object);
        // UnitOfWorkにモックのDbContextを注入する
        _unitOfWork = new UnitOfWork(_mockDbContext.Object);
    }

    /// <summary>
    /// BeginAsync()を呼び出すとトランザクションが開始されることを検証する
    /// </summary>
    [TestMethod("トランザクションが開始される")]
    public async Task BeginAsync_ShouldStartTransaction()
    {
        // トランザクションを開始
        await _unitOfWork.BeginAsync();
        // BeginTransactionAsyncが1回だけ呼ばれたか検証する
        _mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Once);
    }

    /// <summary>
    /// CommitAsync()を呼び出すとトランザクションがコミット・破棄されることを検証する
    /// </summary>
    [TestMethod("トランザクションがコミットされる")]
    public async Task CommitAsync_ShouldCommitTransaction()
    {
        // トランザクションを開始
        await _unitOfWork.BeginAsync();
        // トランザクションをコミット
        await _unitOfWork.CommitAsync();
        // コミットと破棄が正しく呼び出されたか検証
        _mockTransaction.Verify(t => t.CommitAsync(default), Times.Once);
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// RollbackAsync()を呼び出すとトランザクションがロールバック・破棄されることを検証する
    /// </summary>
    [TestMethod("トランザクションがロールバックされる")]
    public async Task RollbackAsync_ShouldRollbackTransaction()
    {
        // トランザクションを開始
        await _unitOfWork.BeginAsync();
        // トランザクションをロールバック
        await _unitOfWork.RollbackAsync();
        // ロールバックと破棄が正しく呼び出されたか検証する
        _mockTransaction.Verify(t => t.RollbackAsync(default), Times.Once);
        _mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }
}