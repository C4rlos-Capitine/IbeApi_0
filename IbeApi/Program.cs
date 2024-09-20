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
builder.Services.AddTransient<IMailService, MailService>();

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}
app.UseStaticFiles(); // This enables serving static files from wwwroot
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



//configuracoes para rotativa
String wwwroot = app.Environment.WebRootPath;

RotativaConfiguration.Setup(wwwroot, "Rotativa");



app.Run();