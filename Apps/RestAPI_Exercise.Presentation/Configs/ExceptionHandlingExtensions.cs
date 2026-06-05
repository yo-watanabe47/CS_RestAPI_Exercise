using RestAPI_Exercise.Presentation.Middlewares;

namespace RestAPI_Exercise.Presentation.Configs;
/// <summary>
/// ExceptionHandlingMiddlewareを登録する拡張メソッドクラス
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// ExceptionHandlingMiddlewareをアプリケーションパイプラインに登録する拡張メソッド
    /// </summary>
    /// <param name="app">IApplicationBuilderのインスタンス</param>
    /// <returns>登録後のIApplicationBuilder</returns>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}