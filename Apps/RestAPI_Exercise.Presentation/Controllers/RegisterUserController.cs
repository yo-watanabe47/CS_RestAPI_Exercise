using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Exceptions;
using RestAPI_Exercise.Application.Usecases.Users.Interfaces;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Presentation.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace RestAPI_Exercise.Presentation.Controllers;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("api/users/register")]
[SwaggerTag("ユーザー登録API")]
public class RegisterUserController : ControllerBase
{
    private readonly IRegisterUserUsecase _usecase;
    private readonly RegisterUserViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[ユーザーを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ</param>
    public RegisterUserController(
        IRegisterUserUsecase usecase,
        RegisterUserViewModelAdapter adapter)
    {
        _usecase = usecase;
        _adapter = adapter;
    }

    [HttpGet("check")]
    [SwaggerOperation(Summary = "ユーザー名、メールアドレスの重複チェック",
                      Description = "ユーザー名、メールアドレスの存在を検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "既に存在する場合")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "ユーザー名、メールアドレスが未入力の場合")]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string? username, [FromQuery, EmailAddress] string? email)
    {
        // ユーザー名もメールアドレスも入力?
        if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "usernameまたはemailのいずれかを指定してください。" });
        }
        try
        {
            await _usecase.ExistsByUsernameOrEmailAsync(username!, email!);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // ユーザー名、メールアドレスが既に存在する場合
            return Conflict(new
            { code = "USER_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// ユーザーの登録
    /// </summary>
    /// <param name="viewModel">ユースケース:[ユーザーを登録する]を実現するViewModel</param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerOperation(Summary = "ユーザーを登録",
                      Description = "ユーザー情報を受け取り、ユーザーを登録する")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "バリデーションエラーまたは業務ルール違反")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "ユーザーが既に存在する場合")]
    [SwaggerResponse(StatusCodes.Status201Created, "登録成功", typeof(User))]
    public async Task<IActionResult> Register(
        [FromBody, SwaggerRequestBody("ユーザー登録用ViewModel", Required = true)]
        RegisterUserViewModel viewModel)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            // プロパティ名をキー、エラーメッセージ配列を値とするディクショナリに変換する
            var details = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0) // エラーがある項目だけを抽出する
                .ToDictionary( // Dictionaryに変換する
                               // キー:プロパティ名 ("Username", "Email" など)
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
            // ユーザーの存在チェック
            await _usecase.ExistsByUsernameOrEmailAsync(viewModel.Username, viewModel.Email);
            // RegisterUserViewModelからUserを復元する
            var user = await _adapter.RestoreAsync(viewModel);
            // ユーザーを登録する
            await _usecase.RegisterUserAsync(user);
            return Created($"/api/users/{user.UserUuid}", user);
        }
        catch (ExistsException ex)
        {
            // 既に存在するユーザーを受信した
            return Conflict(new { code = "USER_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}