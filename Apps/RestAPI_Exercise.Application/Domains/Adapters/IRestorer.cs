namespace RestAPI_Exercise.Application.Domains.Adapters;
/// <summary>
/// 指定のクラス(TTarget)からドメインオブジェクト(TDomain)を復元するインターフェイス
/// </summary>
/// <typeparam name="TDomain">復元するドメインオブジェクトの型</typeparam>
/// <typeparam name="TTarget">対象クラスの型</typeparam
public interface IRestorer<TDomain, TTarget>
{
    /// <summary>
    ///  他のクラスからドメインオブジェクトへの復元する
    /// </summary>
    /// <typeparam name="TTarget">対象クラスの型</typeparam>
    /// <returns>復元したドメインオブジェクト</returns>
    Task<TDomain> RestoreAsync(TTarget target);
}