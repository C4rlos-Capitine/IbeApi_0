using IbeApi;
using IbeApi.Services;
using Microsoft.OpenApi.Models;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Mail settings configuration (ensure this section is in your appsettings.json)
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Register the IMailService implementation
builder.Services.AddScoped<IMailService, MailService>(); // Choose one lifetime
// You can comment out or remove the following line since scoped is typically used.
builder.Services.AddTransient<IMailService, MailService>();

// Obtenha a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("SqlServerDb");
builder.Services.AddSingleton<IHostedService>(new CodigoCleanupService(connectionString));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "quidgest.co.mz",
        Version = "v1",
        Description = "Example of email send",
        Contact = new OpenApiContact
        {
            Name = "quidgest.co.mz",
            Email = "teste@quidgest.co.mz"
        }
    });
});

//builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseStaticFiles(); // This enables serving static files from wwwroot

// Remove or comment out the HTTPS redirection line
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configurations for Rotativa
string wwwroot = app.Environment.WebRootPath;
RotativaConfiguration.Setup(wwwroot, "Rotativa");

// Allow cleartext traffic
app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps)
    {
        // Optionally handle the HTTP requests, e.g., log them or redirect to HTTPS
        // context.Response.Redirect($"https://{context.Request.Host}{context.Request.Path}", true);
    }
    await next.Invoke();
});

app.Run();
