namespace RestAPI_Exercise.Application.Security;
/// <summary>
/// JWT認証に必要な設定値を保持するクラス
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// トークン発行者(iss)
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// トークン利用者(aud)
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// 署名用のシークレットキー  
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// 有効期限(分単位)
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 60;
}