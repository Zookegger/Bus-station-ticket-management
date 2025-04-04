using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Bus_Station_Ticket_Management.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
   var services = scope.ServiceProvider;

   // Get the UserManager and RoleManager
   var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
   var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

   // Create the Admin role if it doesn't exist
   if (!await roleManager.RoleExistsAsync("Admin"))
   {
       await roleManager.CreateAsync(new IdentityRole("Admin"));
   }

   if (!await roleManager.RoleExistsAsync("Employee"))
   {
       await roleManager.CreateAsync(new IdentityRole("Employee"));
   }

   if (!await roleManager.RoleExistsAsync("Customer"))
   {
       await roleManager.CreateAsync(new IdentityRole("Customer"));
   }

   // Create an admin user if it doesn't exist
   var adminUser = await userManager.FindByEmailAsync("admin@example.com");
   if (adminUser == null)
   {
       adminUser = new ApplicationUser
       {
           UserName = "admin@example.com",
           Email = "admin@example.com",
           FullName = "Admin User",
           Gender = "Male",
           DateOfBirth = new DateOnly(1982, 11, 11)
       };
       
       await userManager.CreateAsync(adminUser, "Admin@123");
       await userManager.AddToRoleAsync(adminUser, "Admin");
   }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    ).WithStaticAssets();

    // Admin Area Routing
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    ).WithStaticAssets();

    // Admin Area Routing
    endpoints.MapControllerRoute(
        name: "admin",
        pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}",
        defaults: new { area = "Admin" }
    );

    // Route mặc định
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );

    // Route cho Vehicle
    endpoints.MapControllerRoute(
        name: "vehicle",
        pattern: "Vehicle/{action=Index}/{id?}",
        defaults: new { controller = "Vehicle" }
    );

    // Route cho VehicleType
    endpoints.MapControllerRoute(
        name: "vehicletype",
        pattern: "VehicleType/{action=Index}/{id?}",
        defaults: new { controller = "VehicleType" }
    );
});

app.Run();