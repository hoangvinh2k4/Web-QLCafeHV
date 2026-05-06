using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<CoffeeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<IMergeService, MergeService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<ReviewService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
