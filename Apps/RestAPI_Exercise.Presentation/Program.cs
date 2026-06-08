using RestAPI_Exercise.Presentation.Configs;

var builder = WebApplication.CreateBuilder(args);

// 依存関係(DI)の設定
ApplicationDependencyExtensions
    .AddApplicationDependencies(builder.Services, builder.Configuration);
// JWT認証ミドルウェアをサービス登録する
builder.Services.AddJwtAuthentication(builder.Configuration);
// Swagger(Open API)のサービス登録する
builder.Services.AddSwaggerWithJwt();
// WebApplicationを生成する
var app = builder.Build();

// 開発環境のみSwaggerを有効化
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestAPI Exercise v1");
        c.RoutePrefix = string.Empty; // ルートURLでUIを開く

        // UseRequestInterceptor メソッドを使用する
        c.UseRequestInterceptor("(request) => { request.credentials = 'include'; return request; }");
    });
}


// 例外ハンドリングを登録する
app.UseExceptionHandling();
// HTTPリクエストをHTTPSへ自動リダイレクトするKestrelミドルウェアを有効化
app.UseHttpsRedirection();
// HSTSを有効化
app.UseHsts();
// 認証(Authentication)を有効化する
app.UseAuthentication();
// 認可(Authorization)を有効化する
app.UseAuthorization();
// Controllerのルーティングを有効化
app.MapControllers();
// アプリケーションを実行する
app.Run();