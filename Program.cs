// // using Microsoft.AspNetCore.Authentication.JwtBearer;
// // using Microsoft.EntityFrameworkCore;
// // using Microsoft.IdentityModel.Tokens;
// // using Microsoft.OpenApi.Models;
// // using System.Text;
// // using attendance_api.Data;
// // using attendance_api.Services;

// // var builder = WebApplication.CreateBuilder(args);

// // // Add services to the container
// // builder.Services.AddControllers();
// // builder.Services.AddEndpointsApiExplorer();

// // // Configure Swagger with JWT support
// // builder.Services.AddSwaggerGen(c =>
// // {
// //     c.SwaggerDoc("v1", new OpenApiInfo
// //     {
// //         Title = "Attendance Management API",
// //         Version = "v1",
// //         Description = "API for Attendance Management System with Check-In/Check-Out, GPS, Selfie Verification, and PDF Reports"
// //     });

// //     // Add JWT Authentication to Swagger
// //     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
// //     {
// //         Name = "Authorization",
// //         Type = SecuritySchemeType.Http,
// //         Scheme = "Bearer",
// //         BearerFormat = "JWT",
// //         In = ParameterLocation.Header,
// //         Description = "Enter 'Bearer' followed by space and your JWT token"
// //     });

// //     c.AddSecurityRequirement(new OpenApiSecurityRequirement
// //     {
// //         {
// //             new OpenApiSecurityScheme
// //             {
// //                 Reference = new OpenApiReference
// //                 {
// //                     Type = ReferenceType.SecurityScheme,
// //                     Id = "Bearer"
// //                 }
// //             },
// //             new string[] {}
// //         }
// //     });
// // });

// // // Configure Database
// // builder.Services.AddDbContext<ApplicationDbContext>(options =>
// //     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // // Configure JWT Authentication
// // var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// // var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");

// // builder.Services.AddAuthentication(options =>
// // {
// //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
// //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// // })
// // .AddJwtBearer(options =>
// // {
// //     options.TokenValidationParameters = new TokenValidationParameters
// //     {
// //         ValidateIssuer = true,
// //         ValidateAudience = true,
// //         ValidateLifetime = true,
// //         ValidateIssuerSigningKey = true,
// //         ValidIssuer = jwtSettings["Issuer"],
// //         ValidAudience = jwtSettings["Audience"],
// //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
// //         ClockSkew = TimeSpan.Zero
// //     };
// // });

// // builder.Services.AddAuthorization();

// // // Register Services
// // builder.Services.AddScoped<IJwtService, JwtService>();
// // builder.Services.AddScoped<IFileService, FileService>();
// // builder.Services.AddScoped<IPdfService, PdfService>();

// // // Configure CORS
// // builder.Services.AddCors(options =>
// // {
// //     options.AddPolicy("AllowAll", policy =>
// //     {
// //         policy.AllowAnyOrigin()
// //               .AllowAnyMethod()
// //               .AllowAnyHeader();
// //     });
// // });

// // var app = builder.Build();

// // // Configure the HTTP request pipeline
// // if (app.Environment.IsDevelopment())
// // {
// //     app.UseSwagger();
// //     app.UseSwaggerUI(c =>
// //     {
// //         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
// //         c.RoutePrefix = string.Empty; // Swagger at root
// //     });
// // }

// // // Enable static files for selfie storage
// // app.UseStaticFiles();

// // app.UseHttpsRedirection();

// // app.UseCors("AllowAll");

// // app.UseAuthentication();
// // app.UseAuthorization();

// // app.MapControllers();

// // // Create upload directories if they don't exist
// // var selfiePath = Path.Combine(app.Environment.WebRootPath, "uploads", "selfies");
// // if (!Directory.Exists(selfiePath))
// // {
// //     Directory.CreateDirectory(selfiePath);
// // }

// // app.Run();










// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using attendance_api.Data;
// using attendance_api.Services;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // Configure Swagger with JWT support
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Attendance Management API",
//         Version = "v1",
//         Description = "API for Attendance Management System with Check-In/Check-Out, GPS, Selfie Verification, and PDF Reports"
//     });

//     // Add JWT Authentication to Swagger
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter 'Bearer' followed by space and your JWT token"
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
//             new string[] {}
//         }
//     });
// });

// // Configure Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Configure JWT Authentication
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtSettings["Issuer"],
//         ValidAudience = jwtSettings["Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//         ClockSkew = TimeSpan.Zero
//     };
// });

// builder.Services.AddAuthorization();

// // Register Services
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<IPdfService, PdfService>();

// // Configure CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

// var app = builder.Build();

// // Configure the HTTP request pipeline
// // Enable Swagger in all environments (remove for production)
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
//     c.RoutePrefix = "swagger"; // Access at /swagger instead of root
// });

// // Enable static files for selfie storage
// app.UseStaticFiles();

// // Comment out HTTPS redirection for development
// // app.UseHttpsRedirection();

// app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// // Create upload directories if they don't exist
// var selfiePath = Path.Combine(app.Environment.WebRootPath, "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// app.Run();







// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using attendance_api.Data;
// using attendance_api.Services;

// var builder = WebApplication.CreateBuilder(args);

// // ⭐ ADDED: Listen on all network interfaces (for Flutter app connectivity)
// builder.WebHost.UseUrls("http://0.0.0.0:5000");

// // Add services to the container
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // Configure Swagger with JWT support
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Attendance Management API",
//         Version = "v1",
//         Description = "API for Attendance Management System with Check-In/Check-Out, GPS, Selfie Verification, and PDF Reports"
//     });

//     // Add JWT Authentication to Swagger
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter 'Bearer' followed by space and your JWT token"
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
//             new string[] {}
//         }
//     });
// });

// // Configure Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Configure JWT Authentication
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtSettings["Issuer"],
//         ValidAudience = jwtSettings["Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//         ClockSkew = TimeSpan.Zero
//     };
// });

// builder.Services.AddAuthorization();

// // Register Services
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<IPdfService, PdfService>();

// // Configure CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

// var app = builder.Build();

// // Configure the HTTP request pipeline
// // Enable Swagger in all environments (remove for production)
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
//     c.RoutePrefix = "swagger"; // Access at /swagger instead of root
// });

// // Enable static files for selfie storage
// app.UseStaticFiles();

// // Comment out HTTPS redirection for development
// // app.UseHttpsRedirection();

// app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// // Create upload directories if they don't exist
// var selfiePath = Path.Combine(app.Environment.WebRootPath, "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     var ok = db.Database.CanConnect();
//     Console.WriteLine($"✅ DB Connected: {ok}");
// }


// app.Run();






// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using attendance_api.Data;
// using attendance_api.Services;

// var builder = WebApplication.CreateBuilder(args);

// // ⭐ Listen on all network interfaces (for Flutter app connectivity)
// builder.WebHost.UseUrls("http://0.0.0.0:5000");

// // Add services
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // Configure Swagger with JWT support
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Attendance API",
//         Version = "v1",
//         // Description = "API for Attendance Management System with Check-In/Check-Out, GPS, Selfie Verification, and PDF Reports"
//     });

//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter 'Bearer' followed by space and your JWT token"
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

// // ✅ Configure Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // ✅ Configure JWT Authentication
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtSettings["Issuer"],
//         ValidAudience = jwtSettings["Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
//         ClockSkew = TimeSpan.Zero
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

// var app = builder.Build();

// // ✅ Swagger enabled for all environments
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
//     c.RoutePrefix = "swagger";
//         c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);

// });

// // ✅ Enable static files for selfie storage
// app.UseStaticFiles();

// // ❌ Commented out HTTPS redirection for dev
// // app.UseHttpsRedirection();

// app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// // ✅ Create upload directories if they don't exist
// var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// // ✅ DB connectivity test (prints exact error)
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
//         Console.WriteLine("❌ DB Connection Error:\n" + ex.ToString());
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
// using System.Security.Claims;

// var builder = WebApplication.CreateBuilder(args);

// // ⭐ Listen on all network interfaces (for Flutter app connectivity)
// builder.WebHost.UseUrls("http://150.241.244.64:5430/api");

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
//     // For local dev (http)
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

//         // ✅ Important for [Authorize(Roles="admin")]
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
//         Scheme = "bearer",          // ✅ MUST be "bearer" (lowercase)
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

// // ✅ Enable static files for selfie storage
// app.UseStaticFiles();

// // ❌ Commented out HTTPS redirection for dev
// // app.UseHttpsRedirection();

// app.UseCors("AllowAll");

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// // ✅ Create upload directories if they don't exist
// var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
// if (!Directory.Exists(selfiePath))
// {
//     Directory.CreateDirectory(selfiePath);
// }

// // ✅ DB connectivity test (prints exact error)
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
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ✅ CORRECTED: Listen on port 5000 (not database port 5430)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services
builder.Services.AddControllers();
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
    // For local dev (http)
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

        // ✅ Important for [Authorize(Roles="admin")]
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
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
        Scheme = "bearer",          // ✅ MUST be "bearer" (lowercase)
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

var app = builder.Build();

// ✅ Swagger enabled for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1");
    c.RoutePrefix = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

// ✅ Enable static files for selfie storage
app.UseStaticFiles();

// ❌ Commented out HTTPS redirection for dev
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Create upload directories if they don't exist
var selfiePath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "selfies");
if (!Directory.Exists(selfiePath))
{
    Directory.CreateDirectory(selfiePath);
}

// ✅ DB connectivity test (prints exact error)
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