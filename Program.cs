using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DrakimaWebsite.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DrakimaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DrakimaDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddHttpContextAccessor();

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Splash}/{id?}");

// Map public routes
app.MapControllerRoute(
    name: "public",
    pattern: "{controller=Home}/{action}/{id?}",
    defaults: new { controller = "Home" },
    constraints: new { action = "Splash|About|Privacy|Terms|Contact" });

// Map account routes
app.MapControllerRoute(
    name: "account",
    pattern: "{controller=Account}/{action}/{id?}",
    defaults: new { controller = "Account" },
    constraints: new { action = "Login|Register" });

app.Run();