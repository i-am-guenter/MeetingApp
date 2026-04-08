# 1. Create the blank solution
dotnet new sln -n MeetingApp

# 2. Create the class libraries for the inner rings (Core)
dotnet new classlib -n MeetingApp.Domain -f net8.0
dotnet new classlib -n MeetingApp.Application -f net8.0

# 3. Create the class library for external concerns (DB, external APIs)
dotnet new classlib -n MeetingApp.Infrastructure -f net8.0

# 4. Create the Presentation layer (Razor Pages Web App)
dotnet new webapp -n MeetingApp.Web -f net8.0

# 5. Add all generated projects to the solution
dotnet sln add MeetingApp.Domain/MeetingApp.Domain.csproj
dotnet sln add MeetingApp.Application/MeetingApp.Application.csproj
dotnet sln add MeetingApp.Infrastructure/MeetingApp.Infrastructure.csproj
dotnet sln add MeetingApp.Web/MeetingApp.Web.csproj

# 6. Set up the strict project references (Enforcing the Dependency Rule)
# Application depends ONLY on Domain
dotnet add MeetingApp.Application/MeetingApp.Application.csproj reference MeetingApp.Domain/MeetingApp.Domain.csproj

# Infrastructure depends on Application (which transitively includes Domain)
dotnet add MeetingApp.Infrastructure/MeetingApp.Infrastructure.csproj reference MeetingApp.Application/MeetingApp.Application.csproj

# Web depends on Application and Infrastructure
dotnet add MeetingApp.Web/MeetingApp.Web.csproj reference MeetingApp.Application/MeetingApp.Application.csproj
dotnet add MeetingApp.Web/MeetingApp.Web.csproj reference MeetingApp.Infrastructure/MeetingApp.Infrastructure.csproj

dotnet new gitignore -n VisualStudio -o .gitignore
dotnet dev-cert https --trust