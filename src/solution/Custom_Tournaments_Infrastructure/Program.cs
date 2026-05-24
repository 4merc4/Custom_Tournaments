
using Custom_Tournaments_Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Custom_Tournaments_Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Custom_Tournaments_Context")));

// ===== ЗМІНИ В Program.cs =====
// Додай ці рядки після існуючого builder.Services.AddDbContext<Custom_Tournaments_Context>:

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityContext")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Спрощені вимоги до паролю
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<IdentityContext>()
.AddDefaultTokenProviders();

// Налаштування куків
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
// app.UseRouting(); // вже є
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tournaments}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
