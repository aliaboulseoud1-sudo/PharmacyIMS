var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// A global AuthorizeFilter means every controller/action requires a signed-in
// user by default. AccountController's Login/AccessDenied actions are
// explicitly marked [AllowAnonymous] so the login flow itself stays reachable.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// Register EF Core DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- ASP.NET Core Identity (Authentication & Role-Based Access) ----------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Keep default password rules reasonably strict but not excessive
        // for a coursework project; adjust as needed.
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

// Builds the live DB-grounded context string injected into AI prompts
builder.Services.AddScoped<IPharmacyContextService, PharmacyContextService>();

// HttpClient-backed Gemini implementation of the assistant
builder.Services.AddHttpClient<IAiAssistantService, GeminiAiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Allow the chat AJAX calls to send the antiforgery token as a header
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
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

// Authentication MUST run before Authorization so the current user is
// resolved before the global [Authorize] filter evaluates.
app.UseAuthentication();
app.UseAuthorization();

// Dashboard is the landing page of the application
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// ---------- Seed Identity roles & default demo users ----------
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedRolesAndUsersAsync(scope.ServiceProvider);
}

app.Run();
