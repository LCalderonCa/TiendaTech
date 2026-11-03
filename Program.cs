using TiendaTech.Components;
using TiendaTech.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Cliente de la API JHipster (las llamadas salen del servidor Blazor, no del navegador: no requiere CORS)
builder.Services.AddScoped<Sesion>();
builder.Services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:9999/");
    c.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/no-encontrado");
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
