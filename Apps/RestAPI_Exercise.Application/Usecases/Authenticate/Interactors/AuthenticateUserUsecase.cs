using System.Security.Authentication;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Security;
using RestAPI_Exercise.Application.Usecases.Authenticate.Interfaces;

namespace RestAPI_Exercise.Application.Usecases.Authenticate.Interactors;
/// <summary>
/// ユースケース:[ログインする]を実現するインターフェイスの実装
/// </summary>
public class AuthenticateUserUsecase : IAuthenticateUserUsecase
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHashingService _service;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">ドメインオブジェクト:User(ユーザー)のCRUD操作インターフェイス</param>
    /// <param name="service">パスワードのハッシュ化と検証機能を提供するインターフェイス</param>
    public AuthenticateUserUsecase(
        IUserRepository repository, IPasswordHashingService service)
    {
        _repository = repository;
        _service = service;
    }

    /// <summary>
    /// ユーザー名またはメールアドレスとパスワードでログイン認証する
    /// </summary>
    /// <param name="usernameOrEmail">ユーザー名またはメールアドレス</param>
    /// <param name="password">パスワード(平文)</param>
    /// <returns>認証ユーザー</returns>
    /// <exception cref="AuthenticationException">
    ///     ユーザーが存在しないか、パスワードが不一致
    /// </exception>
    public async Task<User> AuthenticateAsync(string usernameOrEmail, string password)
    {
        // ユーザー名またはメールアドレスでユーザーを取得する
        var user = await _repository.SelectByUsernameOrEmailAsync(usernameOrEmail);
        if (user == null)
        {
            throw new AuthenticationException("ユーザーが存在しません。");
        }
        // パスワードを検証する
        if (!_service.Verify(user.Password, password))
        {
            throw new AuthenticationException("パスワードが一致しません。");
        }
        return user; // ユーザーを返す
    }
}