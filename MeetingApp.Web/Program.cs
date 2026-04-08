using MeetingApp.Application.Configuration;
using MeetingApp.Application.Moderators.Commands.SelectNextModerator;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Infrastructure.Identity;
using MeetingApp.Infrastructure.Persistence;
using MeetingApp.Infrastructure.Persistence.Repositories;
using MeetingApp.Web.HostedServices;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration Options Mapping
// Binds the strongly typed MeetingSettings to the appsettings.json section
builder.Services.Configure<MeetingSettings>(
    builder.Configuration.GetSection(MeetingSettings.SectionName));

// 2. Microsoft Identity Web (Entra ID OIDC Integration)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));


builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.ResponseType = OpenIdConnectResponseType.Code;
    
    // Optional but recommended for downstream API calls or token inspection:
    options.SaveTokens = true; 
});


builder.Services.AddAuthorization();

// 3. Razor Pages & Identity UI endpoints
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

// 4. Entity Framework Core (SQLite)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                          ?? "Data Source=meetingapp.db";

builder.Services.AddDbContext<MeetingDbContext>(options =>
    options.UseSqlite(connectionString, sqlOptions => 
    {
        // Explicitly pointing to the Infrastructure project for migrations
        sqlOptions.MigrationsAssembly("MeetingApp.Infrastructure");
    }));

// 5. CQRS via MediatR
builder.Services.AddMediatR(cfg => 
{
    // Scanning the Application assembly using our command as a marker type
    cfg.RegisterServicesFromAssembly(typeof(SelectNextModeratorCommand).Assembly);
});

// 6. Dependency Injection Bindings
builder.Services.AddScoped<IColleagueRepository, ColleagueRepository>();

// GraphService is registered as a Singleton because it uses Application Permissions (Client Credentials Flow)
// and maintains an internal MSAL token cache for the Daemon process.
builder.Services.AddSingleton<IGraphService, EntraIdGraphService>();

// 7. Hosted Services
// Registers the Daemon that syncs the local DB with Entra ID upon application startup
builder.Services.AddHostedService<StartupSyncService>();

var app = builder.Build();

// --- HTTP Request Pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Strict Transport Security
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication validates the token/cookie, Authorization enforces the policies.
// Order is strictly mandatory.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Required to map the internal Microsoft Identity UI endpoints (e.g., login/logout callbacks)
app.MapControllers();

app.Run();