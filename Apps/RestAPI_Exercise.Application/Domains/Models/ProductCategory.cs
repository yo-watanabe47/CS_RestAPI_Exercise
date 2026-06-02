using RestAPI_Exercise.Application.Exceptions;
namespace RestAPI_Exercise.Application.Domains.Models;
/// <summary>
/// 商品カテゴリを表すドメインオブジェクト
/// </summary>
public class ProductCategory
{
    // 業務上の商品カテゴリ識別子
    public string CategoryUuid { get; private set; } = string.Empty;
    // 商品カテゴリ名
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryUuid">UUID（必須）</param>
    /// <param name="name">カテゴリ名（20文字以内）</param>
    public ProductCategory(string categoryUuid, string name)
    {
        ValidateUuid(categoryUuid); // UUIDの形式検証
        CategoryUuid = categoryUuid;
        ValidateName(name);         // カテゴリ名のルール検証
        Name = name;
    }

    /// <summary>
    /// 新規作成用コンストラクタ(UUIDを内部生成)
    /// </summary>
    /// <param name="name">カテゴリ名</param>
    public ProductCategory(string name)
        : this(Guid.NewGuid().ToString(), name){}

    // 商品カテゴリ名の最大長
    private const int MaxLength = 20;
    /// <summary>
    /// カテゴリ名のルール検証
    /// </summary>
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("カテゴリ名は必須です。");
        if (name.Length > MaxLength)
            throw new DomainException($"カテゴリ名は{MaxLength}文字以内である必要があります。");
    }

    /// <summary>
    /// UUIDの形式検証(Guidとして妥当か)
    /// </summary>
    /// <param name="uuid">検証対象</param>
    private void ValidateUuid(string uuid)
    {
        if (!Guid.TryParse(uuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    /// <summary>
    /// カテゴリ名の変更
    /// </summary>
    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    /// <summary>
    /// 識別子の等価性判定
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is ProductCategory other && CategoryUuid == other.CategoryUuid;
    }
    public override int GetHashCode() => CategoryUuid.GetHashCode();

    /// <summary>
    /// インスタンスの内容
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{CategoryUuid}: {Name}";
}