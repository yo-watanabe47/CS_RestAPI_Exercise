using RestAPI_Exercise.Application.Domains.Models;
namespace RestAPI_Exercise.Application.Usecases.Authenticate.Interfaces;
/// <summary>
/// ユースケース:[ログインする]を実現するインターフェイス
/// </summary>
public interface IAuthenticateUserUsecase
{
    /// <summary>
    /// ユーザー名またはメールアドレスとパスワードでログイン認証する
    /// </summary>
    /// <param name="usernameOrEmail">ユーザー名またはメールアドレス</param>
    /// <param name="password">パスワード(平文)</param>
    /// <returns>認証ユーザー</returns>
    /// <exception cref="AuthenticationException">
    ///     ユーザーが存在しないか、パスワードが不一致
    /// </exception>
    Task<User> AuthenticateAsync(string usernameOrEmail, string password);
}