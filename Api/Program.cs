using System.Text;
using Api.Services;
using Application;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DataAccess.EFCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tools;


var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    // Lấy thông tin về các phiên bản API đã được định nghĩa
    var apiVersionDescriptionProvider =
        builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

    // Vòng lặp để tạo một Swagger Document cho mỗi phiên bản API
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = $"API của tôi phiên bản {description.ApiVersion}",
            Version = description.ApiVersion.ToString()
        });
    }
});

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    );
});

#endregion

#region Services

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUriService, UriService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTools();

#endregion

#region API Versioning

// 1. Cấu hình API Versioning với tên mới
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        // Cấu hình cách đọc version từ URL
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
// 2. PHẦN QUAN TRỌNG NHẤT: Dùng AddApiExplorer thay vì AddVersionedApiExplorer
    .AddApiExplorer(options =>
    {
        // Định dạng tên version trong Swagger UI: v1, v2, ...
        options.GroupNameFormat = "'v'VVV";

        // Tự động thay thế tham số version trong route
        options.SubstituteApiVersionInUrl = true;
    });

#endregion

#region Authentication

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // (Tùy chọn) Xác thực Issuer, Audience - nên dùng trong Production
            // ValidateIssuer = true,
            // ValidateAudience = true,
            // ValidAudience = builder.Configuration["Jwt:Audience"],
            // ValidIssuer = builder.Configuration["Jwt:Issuer"],

            //IMPORTANT: Key validation
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
            ),
            ValidateLifetime = true,
        };
    });

#endregion

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Lấy lại thông tin các phiên bản API
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Tạo một endpoint trong Swagger UI cho mỗi phiên bản
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

// app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();