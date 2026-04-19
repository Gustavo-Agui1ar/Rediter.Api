using Microsoft.EntityFrameworkCore;
using Rediter.Api.Repositories;
using Rediter.Api.Services;
using Rediter.Api.Services.UtilitariesServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI (Swagger)
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<Rediter.Api.Data.DataContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(connectionString));

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
    .WithScopedLifetime()
);

//Serviços
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
    .WithScopedLifetime()
);

//BGServices
builder.Services.AddHostedService<ImageCleanupBackgroundService>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();