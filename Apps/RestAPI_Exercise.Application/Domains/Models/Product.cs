using RestAPI_Exercise.Application.Exceptions;
namespace RestAPI_Exercise.Application.Domains.Models;

/// <summary>
/// 商品を表すドメインオブジェクト(集約ルート)
/// </summary>
public class Product
{
    // 業務上の商品識別子（UUID）
    public string ProductUuid { get; private set; } = string.Empty;
    // 商品名（最大30文字）
    public string Name { get; private set; } = string.Empty;
    // 商品価格（0円以上）
    public int Price { get; private set; }
    // 商品カテゴリ（null不可）
    public ProductCategory? Category { get; private set; }
    // 在庫情報（null不可）
    public ProductStock? Stock { get; private set; }

    /// <summary>
    /// 再構築・復元用コンストラクタ（UUID指定）
    /// </summary>
    /// <param name="productUuid">商品UUID</param>
    /// <param name="name">商品名</param>
    /// <param name="price">価格</param>
    /// <param name="category">商品カテゴリ</param>
    /// <param name="stock">在庫</param>
    public Product(string productUuid, string name, int price, ProductCategory category, ProductStock stock)
    {
        ValidateUuid(productUuid); // UUID形式検証
        ProductUuid = productUuid;
        ValidateName(name);        // 商品名検証
        Name = name;
        ValidatePrice(price);      // 価格検証
        Price = price;
        Category = category ?? throw new DomainException("カテゴリは必須です。");
        Stock = stock ?? throw new DomainException("在庫情報は必須です。");
    }

    /// <summary>
    /// 新規作成用コンストラクタ（UUID自動生成）
    /// </summary>
    /// <param name="name">商品名</param>
    /// <param name="price">価格</param>
    /// <param name="category">カテゴリ</param>
    /// <param name="stock">在庫</param>
    public Product(string name, int price, ProductCategory category, ProductStock stock)
        : this(Guid.NewGuid().ToString(), name, price, category, stock)　{}

 /// <summary>
    /// 再構築・復元用コンストラクタ（UUID指定）
    /// </summary>
    /// <param name="productUuid">商品UUID</param>
    /// <param name="name">商品名</param>
    /// <param name="price">価格</param>
    /// <param name="category">商品カテゴリ</param>
    /// <param name="stock">在庫</param>
    public Product(string productUuid, string name, int price)
    {
        ValidateUuid(productUuid); // UUID形式検証
        ProductUuid = productUuid;
        ValidateName(name);        // 商品名検証
        Name = name;
        ValidatePrice(price);      // 価格検証
        Price = price;
    }


    /// <summary>
    /// UUIDの形式検証
    /// </summary>
    private void ValidateUuid(string uuid)
    {
        if (!Guid.TryParse(uuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    // 商品名の最大長
    private const int MaxNameLength = 30;
    /// <summary>
    /// 商品名の検証
    /// </summary>
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("商品名は必須です。");
        if (name.Length > MaxNameLength)
            throw new DomainException($"商品名は{MaxNameLength}文字以内である必要があります。");
    }

    /// <summary>
    /// 商品価格の検証
    /// </summary>
    private void ValidatePrice(int price)
    {
        if (price < 0)
            throw new DomainException("価格は0円以上である必要があります。");
    }

    /// <summary>
    /// 商品名の変更
    /// </summary>
    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    /// <summary>
    /// 価格の変更
    /// </summary>
    public void ChangePrice(int price)
    {
        ValidatePrice(price);
        Price = price;
    }

    /// <summary>
    /// カテゴリの変更
    /// </summary>
    public void ChangeCategory(ProductCategory category)
    {
        Category = category ?? throw new DomainException("カテゴリは必須です。");
    }

    /// <summary>
    /// 在庫の変更
    /// </summary>
    public void ChangeStock(ProductStock stock)
    {
            Stock = stock ?? throw new DomainException("在庫情報は必須です。");
    }

    /// <summary>
    /// 識別子の等価性判定
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is Product other && ProductUuid == other.ProductUuid;
    }
    public override int GetHashCode() => ProductUuid.GetHashCode();

    /// <summary>
    /// インスタンスの内容
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"{ProductUuid}: {Name} , {Price}円 / {Category?.Name ?? "未分類"} , 在庫: {Stock?.Stock ?? 0}";
}