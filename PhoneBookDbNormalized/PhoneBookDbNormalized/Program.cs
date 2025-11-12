using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using PhoneBookDbNormalized.Repositories;
using PhoneBookDbNormalized.Services;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Đăng ký Repository
builder.Services.AddScoped<IPhoneBookRepository, PhoneBookRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PhoneBook API",
        Version = "v1",
        Description = "API cho hệ thống danh bạ điện thoại"
    });

    // Cấu hình Cookie Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "Cookie",
        In = ParameterLocation.Cookie,
        Description = "Đăng nhập bằng Cookie Authentication. Sử dụng endpoint /api/Auth/login để đăng nhập trước."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PhoneBookAuth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.LoginPath = "/api/Auth/login";

        // Quan trọng: Cho phép Swagger sử dụng cookie
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Trả về 401 thay vì redirect cho API
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
// Cấu hình Authorization với Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("IsAdmin", "True"));

    options.AddPolicy("AdminRoles", policy =>
        policy.RequireRole("1", "2", "4", "8", "10", "20"));
});
var app = builder.Build();


// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
    
//}
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Phone Book API v1");
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Phone Book API Documentation";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    options.DisplayRequestDuration();
}); app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();