var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI (Swagger)
builder.Services.AddOpenApi();
builder.Services.AddSingleton<Supabase.Client>(
    _ => new Supabase.Client(
        builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("SupabaseUrl is not configured."),
        builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("SupabaseKey is not configured."),
        new Supabase.SupabaseOptions { 
           AutoRefreshToken = true,
           AutoConnectRealtime = true,  
        }
    )   
);
builder.Services.AddSingleton<Session>();

builder.Services.AddScoped<Rediter.Api.Repositories.UserRepository>();

builder.Services.AddScoped<Rediter.Api.Services.UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();