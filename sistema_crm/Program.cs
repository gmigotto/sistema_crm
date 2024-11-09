
using DinkToPdf.Contracts;
using DinkToPdf;
using MercadoPago.Config;
using sistema_crm.Uteis;

var builder = WebApplication.CreateBuilder(args);

var mercadoPagoAccessToken = builder.Configuration["MercadoPago:AccessToken"];
MercadoPagoConfig.AccessToken = mercadoPagoAccessToken;

builder.Services.AddScoped<AsaasClient>();

// Adicione o serviço de sessão
builder.Services.AddDistributedMemoryCache(); // Usa memória para armazenar dados de sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // Define o cookie como HTTP only
    options.Cookie.IsEssential = true; // Torna o cookie essencial
});
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
var wkhtmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "wkhtmltox", "libwkhtmltox.dll");
CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(wkhtmlPath);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<IVendedorService, VendedorService>();
//builder.Services.AddScoped<IVendedorRepository, VendedorRepository>();

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

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Start}/{id?}");

app.Run();

