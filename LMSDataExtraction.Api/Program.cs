using LMSDataExtraction.Api.Middleware;
using LMSDataExtraction.Application.Interfaces;
using LMSDataExtraction.Infrastructure.Canvas;
using LMSDataExtraction.Infrastructure.Persistence;
using LMSDataExtraction.Infrastructure.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(ConfigureDatabase);

void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseNpgsql(connectionString);
}

// Repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();

// External source adapters (mock for now, swap to live implementations later)
builder.Services.AddScoped<IPortflowSource, PortflowMockSource>();
builder.Services.AddScoped<IFeedPulseSource, FeedPulseMockSource>();
builder.Services.AddScoped<ICompetenceSource, CompetenceMockSource>();

// HBO-i competence tool: singleton so the in-memory state is preserved
// between requests (disappears when the Web App restarts).
builder.Services.AddSingleton<IHboiCompetenceStore, HboiCompetenceMockStore>();

// FeedPulse feedback tool: singleton to hold student responses during a
// session (also reset on restart).
builder.Services.AddSingleton<IFeedPulseFeedbackStore, FeedPulseFeedbackMockStore>();

// Caching
builder.Services.AddMemoryCache();

// Canvas service
builder.Services.AddHttpClient<CanvasService>();
builder.Services.AddScoped<ICanvasService, CanvasService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ConfigureSwagger);

void ConfigureSwagger(SwaggerGenOptions options)
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Token",
        In = ParameterLocation.Header,
        Description = "Voer je Canvas API token in"
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
            new string[] { }
        }
    });
}

var app = builder.Build();

// Automatically create the database tables if they do not exist yet (for Azure deployment)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger altijd ingeschakeld (ook in productie)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<BearerTokenMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();
