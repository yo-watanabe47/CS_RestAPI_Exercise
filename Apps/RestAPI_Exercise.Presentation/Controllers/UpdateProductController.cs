using Microsoft.AspNetCore.Mvc;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.ViewModels;
namespace RestAPI_Exercise.Presentation.Controllers;
/// <summary>
/// ユースケース:[商品を変更する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("api/products/update")]
public class UpdateProductController : ControllerBase
{
    private readonly IUpdateProductUsecase _usecase;
    private readonly UpdateProductViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[商品を変更する]を実現するインターフェイス</param>
    /// <param name="adapter">UpdateProductViewModelからドメインオブジェクト:Productへ変換するアダプタ</param>
    public UpdateProductController(
        IUpdateProductUsecase usecase,
        UpdateProductViewModelAdapter adapter)
    {
        _usecase = usecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 選択された商品Idで商品を取得する取得する
    /// </summary>
    /// <param name="productId">商品Id(UUID)</param>
    /// <returns>該当する商品が存在すればOK(200)、存在しなければNotFound(404)</returns>
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductById(string productId)
    {
        try
        {
            var product = await _usecase.GetProductByIdAsync(productId);
            return Ok(product);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(new
            { code = "PRODUCT_NOT_FOUND", message = ex.Message });
        }
    }

    /// <summary>
    /// 商品が既に存在するかを検証する
    /// </summary>
    /// <param name="productName">検証対象の商品名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateProduct([FromQuery] string productName)
    {
        // 商品名がnullか空白
        if (string.IsNullOrWhiteSpace(productName))
        {
            return BadRequest(new
            { code = "INVALID_PRODUCT_NAME", message = "商品名は必須です。" });
        }
        try
        {
            // 商品名の存在有無を調べる
            await _usecase.ExistsByProductNameAsync(productName);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // 商品が既に存在する場合
            return Conflict(new
            { code = "PRODUCT_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// 商品を変更する
    /// </summary>
    /// <param name="model">商品変更用ViewModel</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Updated([FromBody] UpdateProductViewModel model)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            // プロパティ名をキー、エラーメッセージ配列を値とするディクショナリに変換する
            var details = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0) // エラーがある項目だけを抽出する
                .ToDictionary( // Dictionaryに変換する
                               // キー:プロパティ名 ("Name", "Price" など)
                    kv => kv.Key,
                    // 値: 当該プロパティのエラーメッセージ一覧
                    kv => kv.Value!.Errors
                        // エラーメッセージが空やnullの場合は "Invalid value."に置換する
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value." : e.ErrorMessage)
                        .ToArray()
                );
            return BadRequest(new
            { code = "VALIDATION_ERROR", message = "入力内容に誤りがあります。", details });
        }
        try
        {
            // 商品名の存在有無を調べる
            await _usecase.ExistsByProductNameAsync(model.Name);
            // UpdateProductViewModelからProductを復元する
            var product = await _adapter.RestoreAsync(model);
            // 商品を変更する
            await _usecase.UpdateProductAsync(product);
            return Ok(product);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(
                new { code = "PRODUCT_NOT_FOUND", message = ex.Message });
        }
        catch (ExistsException ex)
        {
            // 商品が既に存在する場合
            return Conflict(
                new { code = "PRODUCT_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(
                new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}