/// <summary>
/// パスワードの再ハッシュが必要な場合にスローされる例外
/// </summary>
public class PasswordRehashNeededException : Exception
{
    public PasswordRehashNeededException() { }

    public PasswordRehashNeededException(string message)
        : base(message) { }

    public PasswordRehashNeededException(string message, Exception innerException)
        : base(message, innerException) { }
}