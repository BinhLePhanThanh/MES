using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorageSettings"));
builder.Services.AddAutoMapper(typeof(MappingProfile));

// model services
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IFormulaService, FormulaService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IFormulaMaterialService, FormulaMaterialService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IFormulaPropertyService, FormulaPropertyService>();
builder.Services.AddScoped<IFormulaLoggingService, FormulaLoggingService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

//storage services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IGenericModelFileService, GenericModelFileService>();

// providers
builder.Services.AddSingleton<IFileSignatureProvider, FileSignatureProvider>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()   // Cho phép tất cả origin
            .AllowAnyMethod()   // Cho phép tất cả phương thức GET, POST, v.v.
            .AllowAnyHeader();  // Cho phép tất cả headers
    });
});
// JwtSettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddControllers();

// Configure the HTTP request pipeline.
builder.Services.AddSwaggerGen(options =>
{
    // Thêm phần mô tả security scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token vào đây theo định dạng: Bearer {your token}"
    });

    //     // Gắn security scheme vào tất cả các endpoint
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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


var app = builder.Build();


// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//     //Tắt kiểm tra foreign key
//     context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

//     // Xóa dữ liệu bảng Formula
//     context.Database.ExecuteSqlRaw("DELETE FROM Formulas;");
//     context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name='Formulas';");

//     // Bật lại foreign key
//     context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
// }
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");


//app.UseHttpsRedirection();
app.MapControllers();


app.UseAuthentication();
app.UseAuthorization();
app.Run();
