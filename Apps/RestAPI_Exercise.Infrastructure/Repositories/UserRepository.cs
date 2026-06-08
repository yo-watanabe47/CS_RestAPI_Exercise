using Microsoft.EntityFrameworkCore;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Contexts;
namespace RestAPI_Exercise.Infrastructure.Repositories;
/// <summary>
/// ドメインオブジェクト:User(ユーザー)のCRUD操作インターフェイスの実装
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly UserEntityAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">アプリケーションDbContext</param>
    /// <param name="adapter">ドメインオブジェクト:UserとUserEntityの相互変換</param>
    public UserRepository(AppDbContext context, UserEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// ユーザーを永続化する
    /// </summary>
    /// <param name="user">永続化するユーザー</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(User user)
    {
        try
        {
            var entity = await _adapter.ConvertAsync(user);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザー永続化に失敗しました。 user={user}", ex);
        }
    }

    /// <summary>
    /// ユーザー名またはメールアドレスが既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="email">メールアドレス</param>
    /// <returns>true:存在する false:存在しない</returns>
    public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email)
    {
        try
        {
            return await _context.Users
            .AnyAsync(u => u.Username == username || u.Email == email);
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザー名とメールアドレス存在確認に失敗しました。 username={username} , email={email}", ex);
        }
    }

    /// <summary>
    /// ユーザー名またはパスワードからユーザーを取得する
    /// </summary>
    /// <param name="usernameOrEmail">ユーザー名またはメールアドレス</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    public async Task<User?> SelectByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var entity = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        return entity != null ? await _adapter.RestoreAsync(entity) : null;
    }

    /// <summary>
    /// メールアドレスからユーザーを取得する(ログイン用)
    /// </summary>
    /// <param name="email">メールアドレス</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    public async Task<User?> SelectByEmailAsync(string email)
    {
        try
        {
            var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
            return entity != null ? await _adapter.RestoreAsync(entity) : null;
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"メールアドレスでのユーザー取得に失敗しました。 email={email}", ex);
        }
    }

    /// <summary>
    /// ユーザーId(UUID)からユーザーを取得する
    /// </summary>
    /// <param name="useruuid">ユーザーId(UUID)</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    public async Task<User?> SelectByIdAsync(string useruuid)
    {
        try
        {
            var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserUuid == useruuid);
            return entity != null ? await _adapter.RestoreAsync(entity) : null;
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザーIdでのユーザー取得に失敗しました。 userId={useruuid}", ex);
        }
    }

    /// <summary>
    /// 引数に指定されたユーザーIdでユーザーを削除する
    /// </summary>
    /// <param name="userId">ユーザーId(UUID)</param>
    /// <returns>true:削除成功 false:削除対象が存在しない</returns>
    public async Task<bool> DeleteByUserIdAsync(string userId)
    {
        try
        {
            // 削除対象のユーザーを取得する
            var entity = await _context.Users.SingleOrDefaultAsync(u => u.UserUuid == userId);
            if (entity is null)
            {
                return false; // 該当ユーザーが存在しない場合はfalseを返す
            }
            // ユーザーを削除する
            _context.Users.Remove(entity);
            // 削除結果をデータベースに反映させる
            await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex){
            // InternalExceptionにラップしてスローする
            throw new InternalException(
                $"Id:{userId}のユーザー削除中に予期しないエラーが発生しました。", ex);
        }
    }
}