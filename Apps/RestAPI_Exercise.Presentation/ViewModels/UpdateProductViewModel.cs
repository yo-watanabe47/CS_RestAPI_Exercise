using System.ComponentModel.DataAnnotations;

namespace RestAPI_Exercise.Presentation.ViewModels;
/// <summary>
/// ユースケース:[商品を変更する]を実現するViewModel
/// </summary>
public class UpdateProductViewModel
{
    // 商品Id(UUID)
    [Required(ErrorMessage = "商品Idは必須です。")]
    [RegularExpression(
    "^[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}$",
    ErrorMessage = "商品IdはUUID形式で指定してください。")]
    public string ProductId { get; set; } = string.Empty;
    // 商品名
    [Required(ErrorMessage = "商品名は必須です。")]
    [StringLength(30, ErrorMessage = "商品名は{1}文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;
    // 単価
    [Required(ErrorMessage = "単価は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "単価は0以上の整数を指定してください。")]
    public int Price { get; set; }
    // 在庫数
    [Required(ErrorMessage = "在庫数は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上の整数を指定してください。")]
    public int Stock { get; set; }
}