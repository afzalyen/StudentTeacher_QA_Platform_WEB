using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using test_dotnet1.Areas.Identity;
using test_dotnet_Data_Access.Identity;
using test_dotnet1_Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity services with ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true if you require email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure application cookie options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Redirect to login if not authenticated
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Redirect for access denied
});

// Add services to the container for MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Error handling for production
}

// Middleware configuration
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Enable authentication
app.UseAuthorization();  // Enable authorization

// Call the SeedSuperAdmin method to create a super admin user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeed.SeedSuperAdmin(userManager); // Call the seeding method
}

// Configure routes for controllers and Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run(); // Start the application
