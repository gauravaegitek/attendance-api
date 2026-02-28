// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using attendance_api.Data;
// using attendance_api.Services;
// using attendance_api.Middleware;
// using System.Security.Claims;

// var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:5000");

// // Add services
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // ✅ Configure Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // ✅ JWT Settings
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
// var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
// var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

// // ✅ Configure JWT Authentication
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.RequireHttpsMetadata = false;
//     options.SaveToken = true;

//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,

//         ValidIssuer = issuer,
//         ValidAudience = audience,

//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//         ClockSkew = TimeSpan.Zero,

//         RoleClaimType = ClaimTypes.Role,
//         NameClaimType = ClaimTypes.NameIdentifier
//     };
// });

// builder.Services.AddAuthorization();

// // ✅ Register Services
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<IPdfService, PdfService>();

// // ✅ Configure CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

// // ✅ Swagger with JWT support
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Attendance API",
//         Version = "v1"
//     });

//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter: Bearer {your JWT token}"
//     });

//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
// });

// var app = builder.Build();

// // ✅ Swagger enabled for all environments
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
//     c.RoutePrefix = "swagger";
//     c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
// });

// // ✅ Enable static files
// app.UseStaticFiles();

// app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

// // ✅ Device + Token Validation Middleware
// app.UseMiddleware<DeviceValidationMiddleware>();

// app.MapControllers();

// // ✅ Create upload directories
// var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// // ✅ DB connectivity test
// using (var scope = app.Services.CreateScope())
// {
//     try
//     {
//         var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//         db.Database.OpenConnection();
//         Console.WriteLine("✅ DB Connected: True");
//         db.Database.CloseConnection();
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine("❌ DB Connection Error:\n" + ex);
//     }
// }

// app.Run();








// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using attendance_api.Data;
// using attendance_api.Services;
// using attendance_api.Middleware;
// using System.Security.Claims;

// var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:5000");

// // Add services
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // ✅ Configure Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // ✅ JWT Settings
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
// var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
// var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

// // ✅ Configure JWT Authentication
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.RequireHttpsMetadata = false;
//     options.SaveToken = true;

//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,

//         ValidIssuer = issuer,
//         ValidAudience = audience,

//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//         ClockSkew = TimeSpan.Zero,

//         RoleClaimType = ClaimTypes.Role,
//         NameClaimType = ClaimTypes.NameIdentifier
//     };

//     // ✅ 401 ka custom message
//     options.Events = new JwtBearerEvents
//     {
//         OnChallenge = async context =>
//         {
//             context.HandleResponse();
//             context.Response.StatusCode = 401;
//             context.Response.ContentType = "application/json";
//             await context.Response.WriteAsync(
//                 System.Text.Json.JsonSerializer.Serialize(new
//                 {
//                     success = false,
//                     message = "Unauthorized. Please login to continue."
//                 })
//             );
//         }
//     };
// });

// builder.Services.AddAuthorization();

// // ✅ Register Services
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<IPdfService, PdfService>();

// // ✅ Configure CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

// // ✅ Swagger with JWT support
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Attendance API",
//         Version = "v1"
//     });

//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter: Bearer {your JWT token}"
//     });

//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
// });

// // ✅ App Build — इसके बाद ही app.Use... होगा
// var app = builder.Build();

// // ✅ 403 Custom Message
// app.UseStatusCodePages(async context =>
// {
//     if (context.HttpContext.Response.StatusCode == 403)
//     {
//         context.HttpContext.Response.ContentType = "application/json";
//         await context.HttpContext.Response.WriteAsync(
//             System.Text.Json.JsonSerializer.Serialize(new
//             {
//                 success = false,
//                 message = "Access denied. You do not have permission to perform this action."
//             })
//         );
//     }
// });

// // ✅ Swagger — all environments
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
//     c.RoutePrefix = "swagger";
//     c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
// });

// // ✅ Static Files
// app.UseStaticFiles();

// // ✅ CORS
// app.UseCors("AllowAll");

// // ✅ Authentication & Authorization
// app.UseAuthentication();
// app.UseAuthorization();

// // ✅ Device + Token Validation Middleware
// app.UseMiddleware<DeviceValidationMiddleware>();

// // ✅ Map Controllers
// app.MapControllers();

// // ✅ Create upload directories
// var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// // ✅ DB connectivity test
// using (var scope = app.Services.CreateScope())
// {
//     try
//     {
//         var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//         db.Database.OpenConnection();
//         Console.WriteLine("✅ DB Connected: True");
//         db.Database.CloseConnection();
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine("❌ DB Connection Error:\n" + ex);
//     }
// }

// app.Run();










using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using attendance_api.Data;
using attendance_api.Services;
using attendance_api.Middleware;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services
// ✅ JSON Property Order enable — message sabse upar aayega response mein
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// ✅ Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

// ✅ Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    // ✅ 401 ka custom message
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Unauthorized. Please login to continue.",
                    success = false
                })
            );
        }
    };
});

builder.Services.AddAuthorization();

// ✅ Register Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// ✅ Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Attendance API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ✅ App Build
var app = builder.Build();

// ✅ 403 Custom Message
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Access denied. You do not have permission to perform this action.",
                success = false
            })
        );
    }
});

// ✅ Swagger — all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
    c.RoutePrefix = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

// ✅ Static Files
app.UseStaticFiles();

// ✅ CORS
app.UseCors("AllowAll");

// ✅ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Device + Token Validation Middleware
app.UseMiddleware<DeviceValidationMiddleware>();

// ✅ Map Controllers
app.MapControllers();

// ✅ Create upload directories
var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
if (!Directory.Exists(selfiePath))
{
    Directory.CreateDirectory(selfiePath);
}

// ✅ DB connectivity test
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.OpenConnection();
        Console.WriteLine("✅ DB Connected: True");
        db.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ DB Connection Error:\n" + ex);
    }
}

app.Run();