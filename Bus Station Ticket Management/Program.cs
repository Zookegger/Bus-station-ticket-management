using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.DataProtection;
using Google.Apis.Auth.AspNetCore3;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Services;
using Bus_Station_Ticket_Management.Services.Email;
using Bus_Station_Ticket_Management.Services.QRCode;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.Utilities;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ==================== Service Registration ====================

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bus Station Ticket Management API",
        Version = "v1",
        Description = "API for Bus Station Ticket Management System"
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        // Try alternative path
        var altXmlPath = Path.Combine(AppContext.BaseDirectory, "bin", "Debug", "net9.0", xmlFile);
        if (System.IO.File.Exists(altXmlPath))
        {
            c.IncludeXmlComments(altXmlPath);
        }
    }

    // Add security definitions
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Hosted Background Services
builder.Services.AddHostedService<ExpiredPaymentCleanupService>();
builder.Services.AddHostedService<UpdateTripStatusService>();
builder.Services.AddHostedService<UpdateCouponStatusService>();

// Payment Configuration
builder.Services.AddScoped<VnPaymentService>();
builder.Services.Configure<VnPaymentSetting>(config.GetSection("Payment:VnPayment"));

// Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Keys\"))
    .SetApplicationName("BusStationTicketApp");

// Email
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IEmailBackgroundQueue, EmailBackgroundQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();

// Session & Cookie Policy
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax; // Ensure cookies are sent with cross-site requests
    options.Secure = CookieSecurePolicy.Always;       // Use secure cookies
    options.CheckConsentNeeded = context => false;
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
);

// Google People API helper
builder.Services.AddHttpClient<GooglePeopleApiHelper>();
builder.Services.AddScoped<GooglePeopleApiHelper>();

// ==================== Authentication ====================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
   if (config is not null &&
       !string.IsNullOrEmpty(config["Authentication:Google:ClientId"]) &&
       !string.IsNullOrEmpty(config["Authentication:Google:ClientSecret"]))
   {
       options.ClientId = config["Authentication:Google:ClientId"] ?? string.Empty;
       options.ClientSecret = config["Authentication:Google:ClientSecret"] ?? string.Empty;
   }
   else
   {
       System.Diagnostics.Debug.WriteLine("[Google Authentication] ClientId or ClientSecret is not set");
   }

   options.CallbackPath = "/signin-google";

   // // Request extra scopes for additional profile details.
   // options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
   // options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");

   // // Request access to birthday, phone number, address, and gender information.
   // options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
   // options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
   // options.Scope.Add("https://www.googleapis.com/auth/user.addresses.read");
   // options.Scope.Add("https://www.googleapis.com/auth/user.gender.read");

   // options.SaveTokens = true;
})
.AddFacebook(options =>
{
   if (config is not null &&
       !string.IsNullOrEmpty(config["Authentication:Google:ClientId"]) &&
       !string.IsNullOrEmpty(config["Authentication:Google:ClientSecret"]))
   {
       options.AppId = config["Authentication:Google:ClientId"] ?? string.Empty;
       options.AppSecret = config["Authentication:Google:ClientSecret"] ?? string.Empty;
   }
   else
   {
       System.Diagnostics.Debug.WriteLine("[Facebook Authentication] AppId or AppSecret is not set");
   }

   options.CallbackPath = "/signin-facebook";
});

// ==================== Identity ====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromHours(6); // Set to 6 hours, or whatever you prefer
});

// ==================== MVC & Razor Pages ====================
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizePage("/Account/ExternalLogin");
    });

builder.Services.AddControllersWithViews();

// Add API services
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

// Add QR Code Service
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

// Add Chatbot Service
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// ==================== Application Setup ====================
var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Employee", "Conductor", "Driver", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FullName = "Admin User",
            PhoneNumber = "0902124234",
            Gender = "Male",
            DateOfBirth = new DateOnly(1982, 11, 11),
            EmailConfirmed = true,
            Address = "123 ABC Street",
        };

        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// ==================== Middleware ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bus Station Ticket Management API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();
}

builder.Configuration.AddUserSecrets<Program>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();
app.UseCors();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

// Middleware to handle ngrok requests
app.Use(async (context, next) =>
{
    if (context.Request.Host.Value.Contains("ngrok.io"))
    {
        context.Request.Headers["Host"] = "localhost";
    }
    await next();
});

// Redirect short /Admin to actual Admin panel route
app.Use(async (context, next) =>
{
    switch (context.Request.Path.Value?.ToLowerInvariant())
    {
        case "/admin":
            context.Response.Redirect("/Admin/Home/Dashboard");
            return;

        // You can add more path-based cases here in the future
        // case "/example":
        //     context.Response.Redirect("/some/other/path");
        //     return;

        default:
            await next();
            break;
    }
});

// ==================== Routing ====================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
