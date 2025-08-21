using Azure.Core;
using Azure.Identity;
using Homework; 
using Homework.Repository;
using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using Homework.Services;
using Homework.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

using var httpClient = new HttpClient();
var ip = httpClient.GetStringAsync("https://api.ipify.org").Result;
Console.WriteLine("Public IP: ____________________ " + ip);

builder.Configuration.AddCustomConfiguration(builder.Environment);

//builder.Services.Configure<DbSettings>(
//    builder.Configuration.GetSection("ConnectionStrings"));

//builder.Services.AddDbContext<TestDbContext>((sp, options) =>
//{
//    var dbSettings = sp.GetRequiredService<IOptions<DbSettings>>().Value;
//    options.UseSqlServer(dbSettings.DefaultConnection);
//});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");  // For Database

// -- 0 -- when Local conn string is fixed
builder.Services.AddDbContext<TestDbContext>(options =>
    options.UseSqlServer(connectionString));
//   .UseLazyLoadingProxies()); // For Lazy loading


// when using Azure sql and browser based auth
//string server = "homeworkappdb.database.windows.net";
//string database = "TestDb";

// --2 -- Force login in the SQL Server's tenant using app reg and other login details.
//string tenantId = "13b90aa9-fd62-473c-a929-03fb65181677"; // (N) from Azure Portal → Azure AD → Properties
//string clientId = "b95c443d-b028-4c5d-891b-86e4b0d717aa"; // (N) from Azure Portal → App registration → Properties
//var clientSecretAppReg = ""//
//var credential = new InteractiveBrowserCredential(
//    new InteractiveBrowserCredentialOptions             // login is required to connect with the Azure SQL (user v- is added in the allowed list)
//    {
//        TenantId = tenantId,              // 👈 Important
//        RedirectUri = new Uri("http://localhost")
//    }
//);

//var credential = new DefaultAzureCredential();

//string[] scopes = { "https://database.windows.net/.default" };
//var token = credential.GetToken(new TokenRequestContext(scopes));

//using var connection = new SqlConnection(
//    $"Server=tcp:{server},1433;Database={database};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
//);
//connection.AccessToken = token.Token;

//connection.Open();
//Console.WriteLine("Connected successfully!");

//  --3-----here we use third way - its using client id of app reg, and secret value. using it to auth the sql server

//var credential = new ClientSecretCredential(
//    tenantId: tenantId,
//    clientId: clientId,
//    clientSecret: clientSecretAppReg
//);

//var token = credential.GetToken(
//    new Azure.Core.TokenRequestContext(
//        new[] { "https://database.windows.net/.default" }
//    )
//);

//var connection = new SqlConnection(new SqlConnectionStringBuilder
//{
//    DataSource = server,
//    InitialCatalog = database,
//    //AccessToken = token.Token
//}.ConnectionString);

//connection.AccessToken = token.Token;

//builder.Services.AddDbContext<TestDbContext>(options =>
//    options.UseSqlServer(connection));



builder.Services.AddSingleton<JwtHelper>();

builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        /*
         ReferenceHandler.Preserve: Crucial for safely serializing object graphs with circular references, 
                preventing stack overflows, and managing object identity in the JSON.
        WriteIndented = true: Makes your JSON API responses easy to read during development and testing.
        */
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ✅ Add Authentication BEFORE Build()      // for cookie authentication
//builder.Services.AddAuthentication("HomeworkAppCookie")
//               .AddCookie("HomeworkAppCookie", options =>
//               {
//                   options.Cookie.HttpOnly = true;
//                   options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Always for https
//                   options.Cookie.SameSite = SameSiteMode.None; // strict
//                   options.LoginPath = "/api/auth/login";
//                   options.AccessDeniedPath = "/denied";
//                   options.ExpireTimeSpan = TimeSpan.FromDays(7);

//                   // 👇 Prevent automatic redirect on unauthorized access
//                   options.Events.OnRedirectToLogin = context =>
//                   {
//                       context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                       return Task.CompletedTask;
//                   };
//               });

// this will work if we use HttpContext.SignInAsync() method which is loginByCookie in AuthController, 
// but we are using JWT tokens, so this is not needed
builder.Services.AddAuthentication("HomeworkAppCookie")
    .AddCookie("HomeworkAppCookie", options =>
    {
        options.Cookie.Name = "HomeworkAppCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict; // can be None
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/denied";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    //options.HeaderName = "X-XSRF-TOKEN"; // Header name for the token
    options.Cookie.HttpOnly = false; // important!
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "MyAppRedis_";
});

builder.Services.AddOutputCache();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISender, SendSms>();
builder.Services.AddScoped<ISender, SendEmail>();

var app = builder.Build();

// Configure middleware
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();    // ✅ MUST come after Build()
app.UseAuthorization();
app.UseResponseCaching();
app.UseOutputCache();

app.Map("/", () => "open --> /swagger/index.html");
app.Map("/GetOk", () => "ok");

app.MapGet("/test", (HttpContext context) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'");
    return Results.Text("Test CSP header");
});

app.Use(async (context, next) =>
{
    Console.WriteLine("-xxxxxxxxxx-----------------------");
    await next();
});


app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self' http://localhost:3000; " +
        "frame-src 'none';");

    await next();
});


app.MapControllers();

app.Use((context, next) =>
{
    var cookieHeader = context.Request.Headers["Cookie"].ToString();
    Console.WriteLine($"cookieHeader : ");
    return next();
});

app.UseMiddleware<ExceptionMiddleware>();

app.Run();

static class TestConn // class can be created after app.Run() or Top level statement
{
    public static string Myconn { get; set; }
}