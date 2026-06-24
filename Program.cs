using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using RabbitMQ.Client;
using Rediter.Api.Data;
using Rediter.Api.Hubs;
using Rediter.Api.Infrastructure;
using Rediter.Api.Infrastructure.Notifications;
using Rediter.Api.Models.Users;
using Rediter.Api.Repositories;
using Rediter.Api.Repositories.Users;
using Rediter.Api.Services.Dispatchers;
using Rediter.Api.Services.Users;
using Rediter.Api.Services.UtilitariesServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Logs

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    options.SingleLine = true;
});

Console.WriteLine("====================================");
Console.WriteLine("[STARTUP] Inicializando Rediter API...");
Console.WriteLine($"[STARTUP] Ambiente: {builder.Environment.EnvironmentName}");
Console.WriteLine("====================================");

#endregion

#region Serviços Base

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();

#endregion

#region Firebase

Console.WriteLine("[Firebase] Inicializando Firebase Admin SDK...");

string firebasePath = builder.Configuration["GoogleSettings:FirebasePath"]!;

string fullFirebasePath = Path.Combine(builder.Environment.ContentRootPath, firebasePath);

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(fullFirebasePath)
});

Console.WriteLine("[Firebase] Firebase inicializado.");

#endregion

#region MediatR
Console.WriteLine("[DI] Registrando MediatR (Handlers e Eventos)...");

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

Console.WriteLine("[DI] MediatR registrado.");
#endregion

#region RabbitMQ

Console.WriteLine("[RabbitMQ] Criando conexão...");

var factory = new ConnectionFactory
{
    HostName = "localhost"
};

var rabbitConnection = await factory.CreateConnectionAsync();

Console.WriteLine($"[RabbitMQ] Conectado: {rabbitConnection.IsOpen}");

builder.Services.AddSingleton<IConnection>(rabbitConnection);

builder.Services.AddScoped<INotificationDispatcher, RabbitMQNotificationDispatcher>();

builder.Services.AddHostedService<NotificationListener>();
builder.Services.AddHostedService<ChatMessageListener>();

Console.WriteLine("[RabbitMQ] Serviços registrados.");

#endregion

#region Interceptors

builder.Services.AddScoped<NotificationInterceptor>();

Console.WriteLine("[EF] Interceptors registrados.");

#endregion

#region Oracle

Console.WriteLine("[Oracle] Configurando Oracle Cloud...");

string connectionString =
    builder.Configuration.GetConnectionString("OracleCloudDb")!;

string walletPath = builder.Environment.IsDevelopment()
    ? builder.Configuration["OracleWalletPathLocal"]!
    : builder.Configuration["OracleWalletPath"]!;

OracleConfiguration.TnsAdmin = walletPath;
OracleConfiguration.WalletLocation = walletPath;
OracleConfiguration.SqlNetWalletOverride = true;

Console.WriteLine($"[Oracle] Wallet: {walletPath}");

builder.Services.AddDbContext<DataContext>((serviceProvider, options) =>
{
    var interceptor =
        serviceProvider.GetRequiredService<NotificationInterceptor>();

    options
        //.UseLazyLoadingProxies() // Evite usar se não precisar
        .UseOracle(connectionString, b =>
            b.UseOracleSQLCompatibility(
                OracleSQLCompatibility.DatabaseVersion21))
        .AddInterceptors(interceptor);
});

Console.WriteLine("[Oracle] DbContext registrado.");

#endregion

#region Scrutor - Repositories

Console.WriteLine("[DI] Registrando repositories...");

builder.Services.Scan(scan => scan
    .FromAssemblyOf<UserRepository>()
    .AddClasses(classes => classes
        .Where(type =>
            !type.IsAbstract &&
            type.BaseType != null &&
            type.BaseType.IsGenericType &&
            type.BaseType.GetGenericTypeDefinition() ==
                typeof(EntityRepository<>)))
    .AsSelf()
    .WithScopedLifetime());

Console.WriteLine("[DI] Repositories registrados.");

#endregion

#region Scrutor - Services

Console.WriteLine("[DI] Registrando services...");

builder.Services.Scan(scan => scan
    .FromAssemblyOf<UserService>()
    .AddClasses(classes => classes
        .Where(type =>
            type.Name.EndsWith("Service") &&
            type.Name != "BaseService" &&
            !type.IsAbstract &&
            !typeof(BackgroundService).IsAssignableFrom(type)))
    .AsSelf()
    .WithScopedLifetime());

builder.Services.AddHostedService<ImageCleanupBackgroundService>();

Console.WriteLine("[DI] Services registrados.");

#endregion

#region JWT

Console.WriteLine("[JWT] Configurando autenticação...");

var key = Encoding.ASCII.GetBytes(
    builder.Configuration["JwtSettings:Secret"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(key),

                ValidateIssuer = false,
                ValidateAudience = false
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                var path =
                    context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/Hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

Console.WriteLine("[JWT] Autenticação configurada.");

#endregion

#region Ngrok

builder.Services.AddHostedService<NgrokHostedService>();

#endregion

#region Build App

Console.WriteLine("[APP] Construindo aplicação...");

var app = builder.Build();

Console.WriteLine("[APP] Aplicação construída.");

#endregion

#region OpenAPI

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    Console.WriteLine("[OpenAPI] Endpoint habilitado.");
}

#endregion

#region Middleware

app.UseAuthentication();
app.UseAuthorization();

Console.WriteLine("[Middleware] Authentication configurado.");
Console.WriteLine("[Middleware] Authorization configurado.");

#endregion

#region Database Validation

try
{
    using var scope = app.Services.CreateScope();

    Console.WriteLine("[Oracle] Resolvendo DbContext...");

    var db =
        scope.ServiceProvider.GetRequiredService<DataContext>();

    Console.WriteLine("[Oracle] DbContext resolvido.");

    Console.WriteLine("[Oracle] Testando conexão...");

    bool canConnect =
        await db.Database.CanConnectAsync();

    Console.WriteLine($"[Oracle] Conexão OK: {canConnect}");

    // Somente em DEV
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("[Oracle] Executando migrations...");

        await db.Database.MigrateAsync();

        Console.WriteLine("[Oracle] Migrations concluídas.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("[Oracle] ERRO:");
    Console.WriteLine(ex.ToString());
}

#endregion

#region Database Validation
try
{
    using var scope = app.Services.CreateScope();

    Console.WriteLine("[Oracle] Resolvendo DbContext...");
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    Console.WriteLine("[Oracle] DbContext resolvido.");

    Console.WriteLine("[Oracle] Testando conexão...");
    bool canConnect = await db.Database.CanConnectAsync();
    Console.WriteLine($"[Oracle] Conexão OK: {canConnect}");

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("[Oracle] Executando migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("[Oracle] Migrations concluídas.");
    }

    Console.WriteLine("[Seeding] Verificando e criando Roles e Super Admin...");

    var defaultRole = await db.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Default");
    if (defaultRole == null)
    {
        defaultRole = new Role
        {
            Name = "Default",
            Description = "Usuário comum com permissões padrão do aplicativo"
        };
        db.Set<Role>().Add(defaultRole);
        Console.WriteLine("[Seeding] Role 'Default' inserida no banco.");
    }

    var adminRole = await db.Set<Role>().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
    if (adminRole == null)
    {
        adminRole = new Role
        {
            Name = "SuperAdmin",
            Description = "Administrador máximo com controle total do sistema"
        };
        db.Set<Role>().Add(adminRole);
        Console.WriteLine("[Seeding] Role 'SuperAdmin' inserida no banco.");
    }

    await db.SaveChangesAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserService>();
    var adminEmail = "admin@rediter.com";
    var adminUser = await userManager.GetByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        string rawPassword = "AdminP@ssw0rd2026!";
        string hashedPassword = HashService.HashPassword(rawPassword);

        var newAdmin = new User
        {
            Name = "RediterAdmin",
            Email = adminEmail,
            Password = hashedPassword,
            IsVerified = true
        };

        newAdmin.Roles.Add(adminRole);
        newAdmin.Roles.Add(defaultRole);

        userManager.Insert(newAdmin);

        await userManager.SaveChangesAsync();

        Console.WriteLine("[Seeding] Usuário Super Admin criado e vinculado com sucesso!");
    }
    else
        Console.WriteLine("[Seeding] Super Admin já existente. Ignorando criação.");
}
catch (Exception ex)
{
    Console.WriteLine("[Oracle / Seeding] ERRO CRÍTICO:");
    Console.WriteLine(ex.ToString());
}
#endregion

#region Endpoints
app.MapHub<NotificationHub>("/Hubs/NotificationHub");
Console.WriteLine("[SignalR] NotificationHub mapeado.");

app.MapControllers();
Console.WriteLine("[Controllers] Controllers mapeados.");

app.MapGet("/", () => "Rediter API Running");
#endregion

#region Shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("[SHUTDOWN] Encerrando RabbitMQ...");

    var rabbitConn = app.Services.GetRequiredService<IConnection>();
    rabbitConn.CloseAsync().GetAwaiter().GetResult();

    Console.WriteLine("[SHUTDOWN] RabbitMQ encerrado.");
});
#endregion

Console.WriteLine("====================================");
Console.WriteLine("[READY] Rediter API iniciada.");
Console.WriteLine("[READY] Health: /");
Console.WriteLine("[READY] SignalR: /Hubs/NotificationHub");
Console.WriteLine("====================================");

app.Run(); 