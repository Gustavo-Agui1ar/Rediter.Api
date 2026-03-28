using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI (Swagger)
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<Rediter.Api.Data.DataContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(connectionString));

builder.Services.AddScoped<Rediter.Api.Repositories.UserRepository>();

builder.Services.AddScoped<Rediter.Api.Services.UserService>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();