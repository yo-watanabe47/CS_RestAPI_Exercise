using RestAPI_Exercise.Application.Exceptions;
namespace RestAPI_Exercise.Application.Domains.Models;
/// <summary>
/// アプリケーションユーザーを表すドメインオブジェクト
/// </summary>
public class User
{
    // ユーザーの識別子(UUID)
    public string UserUuid { get; private set; } = string.Empty;
    // ユーザー名
    public string Username { get; private set; } = string.Empty;
    // メールアドレス
    public string Email { get; private set; } = string.Empty;
    // パスワード
    public string Password { get; private set; } = string.Empty;

    /// <summary>
    /// コンストラクタ(既存ユーザー:Idあり）
    /// </summary>
    public User(string useruuid, string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(useruuid) || !Guid.TryParse(useruuid, out _))
            throw new DomainException("ユーザーIdはUUID形式でなければなりません。");
        // ユーザー名のバリデーション
        UsernameValidate(username);
        // メールアドレスのバリデーション
        EmailValidate(email);
        // パスワードのバリデーション
        PasswordValidate(password);
        UserUuid = useruuid;
        Username = username;
        Email = email;
        Password = password;
    }

    /// <summary>
    /// コンストラクタ(新規ユーザー)
    /// </summary>
    public User(string username, string email, string password)
        : this(Guid.NewGuid().ToString(), username, email, password) { }

    /// <summary>
    /// ユーザー名のバリデーション
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <exception cref="DomainException"></exception> <summary>
    private void UsernameValidate(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("ユーザー名は必須です。");
        if (username.Length > 30)
            throw new DomainException("ユーザー名は30文字以内で指定してください。");
    }

    /// <summary>
    /// メールアドレスのバリデーション
    /// </summary>
    /// <param name="email">メールアドレス</param>
    /// <exception cref="DomainException"></exception> <summary>
    private void EmailValidate(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new DomainException("メールアドレスの形式が不正です。");
        if (email.Length > 100)
            throw new DomainException("メールアドレスは100文字以内で指定してください。");
    }

    /// <summary>
    /// パスワードのバリデーション
    /// </summary>
    /// <param name="password">パスワード</param>
    private void PasswordValidate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("パスワードは必須です。");
    }

    /// <summary>
    /// ユーザー名を変更する
    /// </summary>
    public void ChangeUsername(string username)
    {
        // ユーザー名のバリデーション
        UsernameValidate(username);
        Username = username;
    }

    /// <summary>
    /// メールアドレスを変更する
    /// </summary>
    public void ChangeEmail(string email)
    {
        // メールアドレスのバリデーション
        EmailValidate(email);
        Email = email;
    }

    /// <summary>
    /// パスワードハッシュを変更する
    /// </summary>
    public void ChangePassword(string password)
    {
        // パスワードのバリデーション
        PasswordValidate(password);
        Password = password;
    }

    /// <summary>
    /// 識別子の等価性判定
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is User other && UserUuid == other.UserUuid;
    }
    public override int GetHashCode() => UserUuid.GetHashCode();

    // <summary>
    /// インスタンスの内容
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Username:{Username}, Email:{Email}, Password:{Password}";
    }
}