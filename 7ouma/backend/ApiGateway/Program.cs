using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

// ─── JWT ───────────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            ValidateIssuer = true, ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true, ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();

// ─── YARP REVERSE PROXY ────────────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(opt =>
    opt.AddPolicy("7oumaPolicy", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors("7oumaPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

Log.Information("7OUMA API Gateway started on port 8080");
app.Run();
