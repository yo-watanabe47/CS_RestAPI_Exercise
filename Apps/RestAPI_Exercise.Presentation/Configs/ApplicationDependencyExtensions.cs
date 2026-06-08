using Microsoft.EntityFrameworkCore;
using RestAPI_Exercise.Infrastructure.Contexts;
using RestAPI_Exercise.Infrastructure.Adapters;
using RestAPI_Exercise.Infrastructure.Repositories;
using RestAPI_Exercise.Application.Domains.Repositories;
using RestAPI_Exercise.Application.Usecases;
using RestAPI_Exercise.Application.Usecases.Products.Interfaces;
using RestAPI_Exercise.Application.Usecases.Products.Interactors;
using RestAPI_Exercise.Infrastructure.Shared;
using RestAPI_Exercise.Application.Security;
using RestAPI_Exercise.Presentation.Adapters;
using RestAPI_Exercise.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using RestAPI_Exercise.Application.Domains.Models;
using RestAPI_Exercise.Application.Usecases.Users.Interfaces;
using RestAPI_Exercise.Application.Usecases.Users.Interactors;
using RestAPI_Exercise.Application.Usecases.Authenticate.Interfaces;
using RestAPI_Exercise.Application.Usecases.Authenticate.Interactors;


namespace RestAPI_Exercise.Presentation.Configs;
/// <summary>
/// 依存関係(DI)の設定
/// インフラストラクチャ層、アプリケーション層、プレゼンテーション層
/// をまとめて追加する拡張クラス
/// </summary>
public static class ApplicationDependencyExtensions
{
    /// <summary>
    /// アプリ全体の依存関係を一括追加する拡張メソッド
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="config">構成情報</param>
    /// <returns>IServiceCollection(チェーン可能)</returns>
public static IServiceCollection AddApplicationDependencies(
this IServiceCollection services, IConfiguration config)
{
    // インフラストラクチャ層の依存関係を追加
    services.AddInfrastructureDependencies(config);
    // アプリケーション層の依存関係を追加
   services.AddApplicationLayerDependencies(config);
    // プレゼンテーション層の依存関係を追加
    services.AddPresentationLayerDependencies(config);
    return services;
}

    // /// <summary>
    // /// インフラストラクチャ層の依存関係を追加
    // /// </summary>
    // /// <param name="services">依存関係注入(DI)のサービスコレクション</param>
    // /// <param name="config">アプリケーションの設定情報を管理</param>
    // /// <returns></returns>
    // private static IServiceCollection AddInfrastructureDependencies(
    //     this IServiceCollection services, IConfiguration config)
    // {
    //     return services;
    // }

/// <summary>
/// インフラストラクチャ層の依存関係を追加
/// </summary>
/// <param name="services">依存関係注入(DI)のサービスコレクション</param>
/// <param name="config">アプリケーションの設定情報を管理</param>
/// <returns></returns>
private static IServiceCollection AddInfrastructureDependencies(
   this IServiceCollection services, IConfiguration config)
{
        // PostgreSQLの接続文字列を設定ファイルから取得する
        var connectstr = config.GetConnectionString("PostgreSQLConnection");
        // AddDbContextをサービスコレクションに登録する
        services.AddDbContext<AppDbContext>(options =>
        {
            // データベース操作ログをデバッグレベルでコンソールに出力する
            options.LogTo(Console.WriteLine, LogLevel.Debug);
            // PostgreSQLのデータベースを指定された接続文字列を使用して構成
            options.UseNpgsql(connectstr);
        });
            // ドメインオブジェクト:ProductSctockとProductStockEntityの相互変換クラス
    services.AddScoped<ProductStockEntityAdapter>();
    // ドメインオブジェクト:ProductCategoryとProductCategoryEntityの相互変換クラス
    services.AddScoped<ProductCategoryEntityAdapter>();
    // ドメインオブジェクト:ProductとProductEntityの相互変換クラス
    services.AddScoped<ProductEntityAdapter>();
    // ドメインオブジェクト:UserとUserEntityの相互変換クラス
    services.AddScoped<UserEntityAdapter>();
    // 商品、商品カテゴリ、商品在庫オブジェクトの相互変換Factoryクラス
    services.AddScoped<ProductFactory>();
    // ドメインオブジェクト:商品カテゴリのCRUD操作Repositoryインターフェイス
    services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
    // ドメインオブジェクト:商品のCRUD操作Repositoryインターフェイス
    services.AddScoped<IProductRepository, ProductRepository>();
    // ドメインオブジェクト:ユーザーのCRUD操作Repositoryインターフェイス
    services.AddScoped<IUserRepository, UserRepository>(); 
    // Unit of Workパターンを利用したトランザクション制御インターフェイス
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    // JWTの発行・検証インターフェイスの実装
    services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
    return services;
}


    /// <summary>
    /// アプリケーション層の依存関係を追加
    /// </summary>
    /// <param name="services">依存関係注入(DI)のサービスコレクション</param>
    /// <param name="config"></param>
    /// <returns></returns>
    private static IServiceCollection AddApplicationLayerDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IRegisterProductUsecase, RegisterProductUsecase>();
        services.AddScoped<IUpdateProductUsecase, UpdateProductUsecase>();
        services.AddScoped<ISearchProductByKeywordUsecase, SearchProductByKeywordUsecase>();
        // ASP.NET Core Identityのパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        // PBKDF2アルゴリズムを利用したパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHashingService, PBKDF2PasswordHashingService>();
        // ユースケース:[ユーザーを登録する]を実現するインターフェイス
        services.AddScoped<IRegisterUserUsecase, RegisterUserUsecase>();
         // JwtSettingsをバインドしてDIに登録する
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        // ユースケース:[ログインする]を実現するインターフェイス
        services.AddScoped<IAuthenticateUserUsecase, AuthenticateUserUsecase>();
        return services;
    }

    /// <summary>
    /// プレゼンテーション層の依存関係を追加
    /// </summary>
    /// <param name="services">依存関係注入(DI)のサービスコレクション</param>
    /// <param name="config"></param>
    /// <returns></returns>
    private static IServiceCollection AddPresentationLayerDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddScoped<RegisterProductViewModelAdapter>();
        services.AddScoped<UpdateProductViewModelAdapter>();
        services.AddScoped<RegisterUserViewModelAdapter>();
        return services;
    }

    /// <summary>
    /// テストプロジェクトにServiceProviderを提供するヘルパメソッド
    /// 生成されたServiceProviderは、テストクラスで使用されるDIコンテナとして機能する
    /// </summary>
    /// <param name="config"></param>
    /// <param name="configureServices"></param>
    /// <param name="configureLogging"></param>
    /// <returns></returns>
    public static ServiceProvider BuildAppProvider(
       IConfiguration config,
       Action<IServiceCollection>? configureServices = null,
       Action<ILoggingBuilder>? configureLogging = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(b =>
        {
            if (configureLogging is not null) configureLogging(b);
            else b.AddConsole().SetMinimumLevel(LogLevel.Warning);
        });
        services.AddApplicationDependencies(config);
        configureServices?.Invoke(services);

        return services.BuildServiceProvider(validateScopes: true);
    }
}