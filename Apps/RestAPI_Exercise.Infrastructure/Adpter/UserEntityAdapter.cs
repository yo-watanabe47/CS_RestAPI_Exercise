using RestAPI_Exercise.Application.Domains.Adapters;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Entities;
namespace RestAPI_Exercise.Infrastructure.Adapters;
/// <summary>
/// ドメインオブジェクト:UserとUserEntityの相互変換アダプタクラス
/// </summary> 
/// <typeparam name="User">ドメインオブジェクト:User</typeparam>
/// <typeparam name="UserEntity">EFCore:UserEntity</typeparam>
public class UserEntityAdapter :
IConverter<User, UserEntity>, IRestorer<User, UserEntity>
{
    /// <summary>
    /// ドメインオブジェクト:UserをUserEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:User</param>
    /// <returns>EFCore:UserEntity</returns>
    public Task<UserEntity> ConvertAsync(User domain)
    {
        if (domain == null)
        {
            throw new InternalException("引数domainがnullです。");
        }
        var entity = new UserEntity();
        entity.UserUuid = domain.UserUuid;
        entity.Username = domain.Username;
        entity.Email = domain.Email;
        entity.PasswordHash = domain.Password;
        return Task.FromResult(entity);
    }

    /// <summary>
    /// UserEntityからドメインオブジェクト:Userを復元する
    /// </summary>
    /// <param name="target">>EFCore:UserEntity</param>
    /// <returns>ドメインオブジェクト:User</returns>
    public Task<User> RestoreAsync(UserEntity target)
    {
        if (target == null)
        {
            throw new InternalException("引数targetがnullです。");
        }
        var domain = new User(
            target.UserUuid.ToString(),
            target.Username,
            target.Email,
            target.PasswordHash);
        return Task.FromResult(domain);
    }
}