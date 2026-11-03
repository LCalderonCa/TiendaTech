using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace TiendaTech.Services;

/// <summary>
/// Sesion del usuario en el circuito de Blazor. El token JWT se guarda cifrado en el
/// localStorage del navegador para sobrevivir a recargas de pagina.
/// </summary>
public class Sesion(ProtectedLocalStorage storage)
{
    private const string Clave = "tiendatech.sesion";

    public string? Token { get; private set; }
    public string? Login { get; private set; }
    public string? Nombre { get; private set; }
    public int ItemsCarrito { get; private set; }
    public bool Cargada { get; private set; }
    public bool Autenticado => Token is not null;

    public event Action? Cambio;

    public async Task CargarAsync()
    {
        if (Cargada) return;
        try
        {
            var r = await storage.GetAsync<DatosSesion>(Clave);
            if (r.Success && r.Value is not null)
            {
                (Token, Login, Nombre) = (r.Value.Token, r.Value.Login, r.Value.Nombre);
            }
        }
        catch
        {
            // almacenamiento ilegible (cambio de clave de cifrado, etc.): se inicia sin sesion
        }
        Cargada = true;
        Cambio?.Invoke();
    }

    public async Task IniciarAsync(string token, string login, string nombre)
    {
        (Token, Login, Nombre) = (token, login, nombre);
        await storage.SetAsync(Clave, new DatosSesion(token, login, nombre));
        Cambio?.Invoke();
    }

    public async Task CerrarAsync()
    {
        (Token, Login, Nombre, ItemsCarrito) = (null, null, null, 0);
        try { await storage.DeleteAsync(Clave); } catch { /* circuito cerrandose */ }
        Cambio?.Invoke();
    }

    public void ActualizarCarrito(int items)
    {
        ItemsCarrito = items;
        Cambio?.Invoke();
    }

    public record DatosSesion(string Token, string Login, string Nombre);
}
