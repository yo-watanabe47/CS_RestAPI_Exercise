using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestAPI_Exercise.Infrastructure.Entities;
/// <summary>
/// productテーブルに対応するEntity Framework Coreのエンティティ
/// </summary>
[Table("product")]
public class ProductEntity
{
    [Key] // 主キーをマッピング
    [Column("id")]
    // 列名と同じ名称のプロパティなので[Column]は使わない
    public int Id { get; set; }

    [Required] // NOT NUll
    [StringLength(36)] // データ長は36文字
    [Column("product_uuid")]// マッピングする列名
    public string ProductUuid { get; set; } = string.Empty;

    [Column("name")]
    [Required] // NOT NULL
    [StringLength(20)]// データ長は20文字
    // 列名と同じ名称のプロパティなので[Column]は使わない
    public string Name { get; set; } = string.Empty;

    [Column("price")]
    [Required] // NOT NULL
    // 列名と同じ名称のプロパティなので[Column]は使わない
    public int Price { get; set; }

    [Column("category_id")]// マッピングする列名
    public int? ProductCategoryId { get; set; }

    // ProductCategroyエンティティへのナビゲーションプロパティ
    // ProductCategoryIdプロパティの値と外部キー関係にある
    // null許容にし、商品のカテゴリを含めないケースも許可する
    [ForeignKey("ProductCategoryId")]
    public ProductCategoryEntity? ProductCategory { get; set; }

    // 在庫情報（1:1 関係を想定）
    public ProductStockEntity? ProductStock { get; set; }

    public override string ToString()
    {
        return $"Id={Id}, ProductUuid={ProductUuid}, Name={Name}, Price={Price}, " +
               $"Category={ProductCategory?.Name}, Stock={ProductStock?.Stock}";
    }
}