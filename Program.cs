using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PO_Api.Data;
using PO_Api.Hubs;
using PO_Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.WebHost.UseUrls("http://0.0.0.0:7004");
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddSignalR();
//builder.Services.AddScoped<IFileService, FileService>();
// เพิ่มบริการ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        // อนุญาตเฉพาะโดเมนที่ระบุ
        policy.WithOrigins(
                "http://localhost:3000", // React
                "http://192.168.4.11:7004",
                "http://localhost:4200",
                "http://localhost:3001",
                "https://www.ymt-group.com",
                "http://localhost:8080",
                "http://localhost:5174",// Vue
                "http://localhost:5173"// Vue
                ) // Vue
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        // อนุญาตทุกโดเมน (ไม่แนะนำสำหรับ Production)
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtOption =>
{
    jwtOption.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    jwtOption.MapInboundClaims = false;
    jwtOption.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // ดึง token จาก cookie แทน header
            var accessToken = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            // ถ้าไม่มี cookie จะ fallback ใช้ header ตามปกติ
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

    // เพิ่ม Security Definition สำหรับ Bearer JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT token with Bearer prefix (Bearer {token})",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAuthorization();    // ✅ เพิ่มบรรทัดนี้
builder.Services.AddControllers();
var app = builder.Build();
app.UseCors("AllowSpecificOrigins"); // หรือ "AllowAll" ถ้าต้องการอนุญาตทั้งหมด
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication(); // ✅ มาก่อน
app.UseAuthorization();  // ✅ ตามหลัง
app.MapHub<ChatHub>("/hub/chatHub");
//app.MapHub<NotificationHub>("/hub/notification"); // 🧠 route ของ SignalR
app.MapControllers();

app.Run();


