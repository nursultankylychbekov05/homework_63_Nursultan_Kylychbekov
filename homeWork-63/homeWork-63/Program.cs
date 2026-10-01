using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using homeWork_63.Data;
using homeWork_63.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Chat}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        if (!await roleManager.RoleExistsAsync("admin"))
            await roleManager.CreateAsync(new IdentityRole("admin"));

        if (!await roleManager.RoleExistsAsync("user"))
            await roleManager.CreateAsync(new IdentityRole("user"));

        string adminEmail = "admin@chat.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "Admin",
                Email = adminEmail,
                EmailConfirmed = true,
                BirthDate = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc)
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "admin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при создании ролей и администратора.");
    }
}

app.Run();