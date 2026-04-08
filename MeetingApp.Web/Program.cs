using MeetingApp.Application.Configuration;
using MeetingApp.Application.Moderators.Commands.SelectNextModerator;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Infrastructure.Persistence;
using MeetingApp.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// 1. Microsoft Identity Web (Entra ID OIDC Integration - Authentication ONLY)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// 2. Razor Pages & Identity UI endpoints
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

// 3. Entity Framework Core (SQLite)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                          ?? "Data Source=meetingapp.db";

builder.Services.AddDbContext<MeetingDbContext>(options =>
    options.UseSqlite(connectionString, sqlOptions => 
    {
        sqlOptions.MigrationsAssembly("MeetingApp.Infrastructure");
    }));

// 4. CQRS via MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(SelectNextModeratorCommand).Assembly);
});

// 5. Dependency Injection Bindings
builder.Services.AddScoped<IColleagueRepository, ColleagueRepository>();

// Architectural clean-up: IGraphService and StartupSyncService have been permanently removed.

var app = builder.Build();

// --- HTTP Request Pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();