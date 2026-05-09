using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rediter.Api.Repositories;
using Rediter.Api.Infrastructure;
using Rediter.Api.Services;
using Rediter.Api.Services.UtilitariesServices;
using Oracle.ManagedDataAccess.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    options.SingleLine = true;
});

// HttpClient
builder.Services.AddHttpClient();

// Controllers
builder.Services.AddControllers();

// OpenAPI (Swagger)
builder.Services.AddOpenApi();

// Configuração de Conexão: Local vs Cloud
string connectionString;

connectionString = builder.Configuration.GetConnectionString("OracleCloudDb")!;

string walletPath = builder.Configuration["OracleWalletPath"]!;

if (builder.Environment.IsDevelopment())
    walletPath = builder.Configuration["OracleWalletPathLocal"]!;

OracleConfiguration.TnsAdmin = walletPath;
OracleConfiguration.WalletLocation = walletPath;
OracleConfiguration.SqlNetWalletOverride = true;

builder.Services.AddDbContext<Rediter.Api.Data.DataContext>(options =>
    options.UseLazyLoadingProxies()
           .UseOracle(connectionString, b => b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion21)));

// Repositórios
builder.Services.Scan(scan => scan
    .FromAssemblyOf<UserRepository>()
    .AddClasses(classes => classes
        .Where(type =>
            !type.IsAbstract &&
            type.BaseType != null &&
            type.BaseType.IsGenericType &&
            type.BaseType.GetGenericTypeDefinition() == typeof(BaseRepository<>)
        )
    )
    .AsSelf()
    .WithScopedLifetime());

// Serviços
builder.Services.Scan(scan => scan
    .FromAssemblyOf<UserService>()
    .AddClasses(classes => classes
        .Where(type =>
            type.Name.EndsWith("Service") &&
            type.Name != "BaseService" &&
            !type.IsAbstract &&
            !typeof(BackgroundService).IsAssignableFrom(type)
        )
    )
    .AsSelf()
    .WithScopedLifetime());

// BGServices
builder.Services.AddHostedService<ImageCleanupBackgroundService>();

// Autenticacao
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"]!);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Execução de Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Rediter.Api.Data.DataContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.Run();