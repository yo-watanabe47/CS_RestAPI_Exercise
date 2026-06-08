namespace RestAPI_Exercise.Application.Security;
/// <summary>
/// パスワードのハッシュ化と検証機能を提供するインターフェイス
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// 平文のパスワードをハッシュ化する
    /// </summary>
    /// <param name="rawPassword">平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    string Hash(string rawPassword);
    
    /// <summary>
    /// パスワードの比較結果を返す
    /// </summary>
    /// <param name="hashedPassword">ハッシュされたパスワード</param>
    /// <param name="providedPassword">平文のパスワード</param>
    /// <returns>true:一致、false:不一致</returns>
    /// <exception cref="PasswordRehashNeededException">
    /// 　パスワードは一致したが、ハッシュの形式や強度が古い場合にスローされる
    /// </exception>
    bool Verify(string hashedPassword, string providedPassword);
}