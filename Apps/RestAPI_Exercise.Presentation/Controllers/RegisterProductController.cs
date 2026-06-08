using Microsoft.AspNetCore.Mvc;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.ViewModels;
using Microsoft.AspNetCore.Authorization;

using Swashbuckle.AspNetCore.Annotations;
namespace RestAPI_Exercise.Presentation.Controllers;
/// <summary>
/// ユースケース:[新商品を登録する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("api/products/register")]
[SwaggerTag("新商品登録API")]
public class RegisterProductController : ControllerBase
{
    private readonly IRegisterProductUsecase _usecase;
    private readonly RegisterProductViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[新商品を登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ</param>
    public RegisterProductController(
        IRegisterProductUsecase usecase,
        RegisterProductViewModelAdapter adapter)
    {
        _usecase = usecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 商品カテゴリ一覧の取得
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("categories")]
    [SwaggerOperation(Summary = "商品カテゴリ一覧を取得", 
                      Description = "登録可能なすべての商品カテゴリを返します。")]
    [SwaggerResponse(StatusCodes.Status200OK, "カテゴリ一覧", typeof(List<ProductCategory>))]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _usecase.GetCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// 選択された商品カテゴリIdで商品カテゴリを取得する取得する
    /// </summary>
    /// <param name="categoryId">商品カテゴリId(UUID)</param>
    /// <returns>該当するカテゴリが存在すればOK(200)、存在しなければNotFound(404)</returns>
    [Authorize]
    [HttpGet("categories/{categoryId}")]
    [SwaggerOperation(Summary = "商品カテゴリの取得", 
                      Description = "指定された商品カテゴリIdに一致する商品カテゴリを返します。")]
    [SwaggerResponse(StatusCodes.Status200OK, "商品カテゴリが見つかった場合", typeof(ProductCategory))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "該当商品カテゴリが存在しない場合")]
    public async Task<IActionResult> GetCategoryById(string categoryId)
    {
        //nullとスペースのチェックがない
        //UUID形式の確認してない
        try
        {
            var category = await _usecase.GetCategoryByIdAsync(categoryId);
            return Ok(category);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(new
            { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
    }

    /// <summary>
    /// 商品が既に存在するかを検証する
    /// </summary>
    /// <param name="productName">検証対象の商品名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [Authorize]
    [HttpGet("validate")]
    [SwaggerOperation(Summary = "商品名の存在確認", 
                      Description = "商品名が既に存在するかを検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "商品名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "商品名が既に存在する場合")]
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
    /// 新商品を登録する
    /// </summary>
    /// <param name="model">商品登録用ViewModel</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(Summary = "新商品を登録", 
                      Description = "商品情報を受け取り、商品を登録する")]
    [SwaggerResponse(StatusCodes.Status201Created, "登録成功", typeof(Product))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "バリデーションエラーまたは業務ルール違反")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "カテゴリIdが存在しない場合")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "商品が既に存在する場合")]
    public async Task<IActionResult> Register(
        // SwaggerRequestBodyを追加
        [FromBody, SwaggerRequestBody("新商品登録用ViewModel", Required = true)]
        RegisterProductViewModel model)
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
            // 存在しない商品カテゴリを受信した(ミスしている)
            await _usecase.GetCategoryByIdAsync(model.CategoryId);
            // 既に登録済みの商品を受信した(ミスしている)
            await _usecase.ExistsByProductNameAsync(model.Name);
            // RegisterProductViewModelからProductを復元する
            var product = await _adapter.RestoreAsync(model);
            // 商品を永続化する
            await _usecase.RegisterProductAsync(product);
            return Created($"/api/products/{product.ProductUuid}", product.ProductUuid);
            //上の最後のProductUuidを消したほうがいいかも
        }
        catch (ExistsException ex)
        {
            // 既に存在する商品を受信した
            return Conflict(new { code = "PRODUCT_ALREADY_EXISTS", message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            // 存在しない商品カテゴリIdを受信した
            return NotFound(new { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}