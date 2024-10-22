
using MercadoPago.Config;
using sistema_crm.Uteis;

var builder = WebApplication.CreateBuilder(args);

var mercadoPagoAccessToken = builder.Configuration["MercadoPago:AccessToken"];
MercadoPagoConfig.AccessToken = mercadoPagoAccessToken;


// Adicione o servi�o de sess�o
builder.Services.AddDistributedMemoryCache(); // Usa mem�ria para armazenar dados de sess�o
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true; // Define o cookie como HTTP only
    options.Cookie.IsEssential = true; // Torna o cookie essencial
});

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

