using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
namespace RestAPI_Exercise.Application.Tests.Applications.Domains.Models;
/// <summary>
/// ProductStockクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class ProductStockTests
{
    [TestMethod("コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var uuid = Guid.NewGuid().ToString();
        var stock = 10;
        // インスタンスを生成する
        var productStock = new ProductStock(uuid, stock);
        // 商品在庫Idを検証する
        Assert.AreEqual(uuid, productStock.StockUuid);
        // 商品在庫数を検証する
        Assert.AreEqual(stock, productStock.Stock);
    }

    [TestMethod("新規作成の場合UUIDが自動生成される")]
    public void  NewInstance_ShouldGenerateUuidAutomatically()
    {
        // データを用意する
        var stock = 5;
        // インスタンスを生成する
        var productStock = new ProductStock(stock);
        //  商品在庫IdがUUID形式かどうかを検証する
        Assert.IsTrue(Guid.TryParse(productStock.StockUuid, out _));
        // 商品在庫数を検証する
        Assert.AreEqual(stock, productStock.Stock);
    }

    [TestMethod("不正なUUIDの場合、DomainExceptionがスローされる")]
    public void InvalidUuid_ShouldThrowDomainException()
    {
        // 不正なUUID
        var invalidUuid = "abcde";
        var stock = 1;
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new ProductStock(invalidUuid , stock); // インスタンスを生成する
        });
        // 例外メッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("在庫数がマイナスの場合、DomainExceptionがスローされる")]
    public void Stock_WithNegativeValue_ShouldThrowDomainException()
    {
        // データを用意する
        var uuid = Guid.NewGuid().ToString();
        var stock = -1;
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            _ = new ProductStock(uuid, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("在庫数は0以上である必要があります。", ex.Message);
    }

    [TestMethod("有効な在庫数の場合、在庫数を変更できる")]
    public void ChangeStock_WithValidValue_ShouldUpdateStock()
    {
        // インスタンス生成する
        var productStock = new ProductStock(10);
        var newStock = 50;
        // 在庫数を変更する
        productStock.ChangeStock(newStock);
        // 変更結果を検証する
        Assert.AreEqual(newStock, productStock.Stock);
    }

    [TestMethod("マイナスの在庫数で変更した場合、DomainExceptionをスローする")]
    public void ChangeStock_WithNegativeValue_ShouldThrowDomainException()
    {
        // インスタンスを生成する
        var productStock = new ProductStock(10);
        var ex = Assert.ThrowsExactly<DomainException>(() =>
        {
            productStock.ChangeStock(-5);// 在庫数を変更する
        });
        // 例外メッセージを検証する
        Assert.AreEqual("在庫数は0以上である必要があります。", ex.Message);
    }

    [TestMethod("UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid().ToString();
        var stock1 = new ProductStock(uuid, 10);
        var stock2 = new ProductStock(uuid, 20);
        // 等価性を検証する
        var result = stock1.Equals(stock2);
        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod("異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var stock1 = new ProductStock(10);
        var stock2 = new ProductStock(10);
        // 等価性を検証する
        var result = stock1.Equals(stock2);
        // 非等価であることを評価する
        Assert.IsFalse(result);
    }
}