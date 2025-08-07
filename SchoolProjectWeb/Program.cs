var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// === INICIO: Agregar servicios de la sesi�n ===
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// === FIN: Agregar servicios de la sesi�n ===


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// === INICIO: Usar el middleware de la sesi�n ===
app.UseSession();
// === FIN: Usar el middleware de la sesi�n ===

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // === INICIO: Cambiar la ruta inicial al LoginController ===
    pattern: "{controller=Login}/{action=Login}/{id?}");
// === FIN: Cambiar la ruta inicial al LoginController ===

app.Run();