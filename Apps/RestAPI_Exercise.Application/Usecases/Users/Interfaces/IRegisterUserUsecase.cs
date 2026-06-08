using RestAPI_Exercise.Application.Domains.Models;
namespace RestAPI_Exercise.Application.Usecases.Users.Interfaces;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するインターフェイス
/// </summary>
public interface IRegisterUserUsecase
{
    /// <summary>
    /// ユーザー名またはメールアドレスが既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="email">メールアドレス</param>
    /// <exception cref="ExistsException">データが存在する場合にスローされる</exception>
    Task ExistsByUsernameOrEmailAsync(string username, string email);

    /// <summary>
    /// ユーザーを登録する
    /// </summary>
    /// <param name="user">登録対象ユーザー</param>
    /// <returns></returns>
    Task RegisterUserAsync(User user);
}