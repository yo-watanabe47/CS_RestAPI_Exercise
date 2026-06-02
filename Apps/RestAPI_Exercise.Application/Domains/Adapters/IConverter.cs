namespace RestAPI_Exercise.Application.Domains.Adapters;
/// <summary>
/// TDomainに指定されたドメインオブジェクトをTTargetに指定されたクラスに変換するインターフェイス
/// </summary>
/// <typeparam name="TDomain">ドメインオブジェクトの型</typeparam>
/// <typeparam name="TTarget">変換クラスの型</typeparam>
public interface IConverter<TDomain, TTarget>
{
    /// <summary>
    /// TDomainに指定されたドメインオブジェクトをTTargetに指定されたクラスに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト</param>
    /// <returns>変換結果</returns>
    Task<TTarget> ConvertAsync(TDomain domain);
}