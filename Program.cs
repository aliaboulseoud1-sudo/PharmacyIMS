using Microsoft.EntityFrameworkCore;
using PharmacyIMS.Data;
using PharmacyIMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register EF Core DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthorization();

// Dashboard is now the landing page of the application
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
