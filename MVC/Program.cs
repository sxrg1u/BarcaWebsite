var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Jedes POST-Formular braucht ein Anti-Forgery-Token (Schutz vor CSRF)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddSession();

// Datenzugriff (ADO.NET): eine Klasse pro Tabelle, SQL steht nur dort
builder.Services.AddSingleton<MVC.Data.Database>();
builder.Services.AddScoped<MVC.Data.UserRepository>();
builder.Services.AddScoped<MVC.Data.PlayerRepository>();
builder.Services.AddScoped<MVC.Data.TrophyRepository>();
builder.Services.AddScoped<MVC.Data.FavouriteRepository>();
builder.Services.AddScoped<MVC.Data.LineupRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

// Fonts lange cachen: MapStaticAssets schickt sonst "no-cache",
// dann fragt der Browser bei jedem Seitenwechsel neu und die Schrift flackert
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/fonts"))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            return Task.CompletedTask;
        });
    }
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
 