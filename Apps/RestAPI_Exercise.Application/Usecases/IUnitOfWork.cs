namespace RestAPI_Exercise.Application.Usecases;
/// <summary>
/// Unit of Workパターンを利用したトランザクション制御インターフェイス
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// トランザクションを開始する
    /// </summary>
    Task BeginAsync();
    /// <summary>
    /// トランザクションをコミットする
    /// </summary>
    Task CommitAsync();
    /// <summary>
    /// トランザクションをロールバックする
    /// </summary>
    Task RollbackAsync();
}