using System.Text;
using Ecommerce.API.Hateoas;
using Ecommerce.API.Middlewares;
using Ecommerce.Application;
using Ecommerce.Application.Interfaces;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "logs", "api-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

// Camadas da aplicacao
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// HATEOAS
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILinkService, LinkService>();

// Autenticacao JWT
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Secao 'Jwt' ausente no appsettings.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SomenteAdministrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("UsuarioAutenticado", policy => policy.RequireAuthenticatedUser());
});

// Compressao e cache
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("CatalogoPublico", policy => policy.Expire(TimeSpan.FromSeconds(30)));
});

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce API - CP5",
        Version = "v1",
        Description = "API RESTful com Clean Architecture, Operation Result, erros customizados, " +
                      "servico externo (ViaCEP), PageResult, HATEOAS e autenticacao JWT."
    });

    // Botao Authorize no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token devolvido por /api/auth/login (sem a palavra Bearer)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationContext>("banco");

var app = builder.Build();

// Prepara o banco na inicializacao
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationContext>();

        // Usa migrations se existirem; senao cria o schema
        if (context.Database.GetMigrations().Any())
            await context.Database.MigrateAsync();
        else
            await context.Database.EnsureCreatedAsync();

        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(context, hasher, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao preparar o banco de dados");
    }
}

// Pipeline HTTP
app.UseExceptionHandlingMiddleware();

app.UseSerilogRequestLogging(options =>
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms");

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
        c.DocumentTitle = "Ecommerce API - CP5";
    });
}

app.UseHttpsRedirection();
app.UseOutputCache();

// Autenticacao antes da autorizacao
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

try
{
    Log.Information("Iniciando a Ecommerce API (CP5)");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicacao encerrou de forma inesperada");
}
finally
{
    Log.CloseAndFlush();
}

// Necessario para o WebApplicationFactory nos testes funcionais
public partial class Program { }
