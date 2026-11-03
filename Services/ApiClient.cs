using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TiendaTech.Services;

public class ApiException(string mensaje, int status) : Exception(mensaje)
{
    public int Status { get; } = status;
}

/// <summary>Cliente de la API del backend (JHipster). Una funcion por endpoint.</summary>
public class ApiClient(HttpClient http, Sesion sesion)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /* ------------------------------ autenticacion ------------------------------ */

    public async Task IniciarSesionAsync(LoginForm form)
    {
        var token = await Enviar<JsonElement>(HttpMethod.Post, "api/authenticate", form, auth: false);
        var jwt = token.GetProperty("id_token").GetString()!;
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/account");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var cuenta = (await resp.Content.ReadFromJsonAsync<Cuenta>(Json))!;
        await sesion.IniciarAsync(jwt, cuenta.Login, cuenta.FirstName ?? cuenta.Login);
        var carrito = await CarritoAsync();
        sesion.ActualizarCarrito(carrito.CantidadActiva);
    }

    public Task<Perfil> RegistrarAsync(RegistroForm f) =>
        Enviar<Perfil>(HttpMethod.Post, "api/tienda/publico/registro", new
        {
            email = f.Email.Trim(),
            password = f.Password,
            nombres = f.Nombres.Trim(),
            apellidos = f.Apellidos.Trim(),
            tipoDocumento = f.TipoDocumento,
            numeroDocumento = f.NumeroDocumento.Trim(),
            celular = f.Celular.Trim(),
            aceptaPublicidad = f.AceptaPublicidad
        }, auth: false);

    /* ------------------------------ catalogo (publico) ------------------------------ */

    public Task<List<CategoriaNodo>> CategoriasAsync() => Get<List<CategoriaNodo>>("api/tienda/publico/categorias", false);

    public Task<List<MarcaItem>> MarcasAsync() => Get<List<MarcaItem>>("api/tienda/publico/marcas", false);

    public Task<Pagina<ProductoCard>> ProductosAsync(string? categoria, long? marca, string? q, bool ofertas, string? orden, int page, int size = 12)
    {
        var qs = new List<string> { $"page={page}", $"size={size}", $"ofertas={ofertas.ToString().ToLowerInvariant()}" };
        if (!string.IsNullOrWhiteSpace(categoria)) qs.Add("categoria=" + Uri.EscapeDataString(categoria));
        if (marca is not null) qs.Add("marca=" + marca);
        if (!string.IsNullOrWhiteSpace(q)) qs.Add("q=" + Uri.EscapeDataString(q));
        if (!string.IsNullOrWhiteSpace(orden)) qs.Add("orden=" + Uri.EscapeDataString(orden));
        return Get<Pagina<ProductoCard>>("api/tienda/publico/productos?" + string.Join("&", qs), false);
    }

    public Task<ProductoDetalle> ProductoAsync(string slug) => Get<ProductoDetalle>("api/tienda/publico/productos/" + Uri.EscapeDataString(slug), false);

    public Task<List<TiendaItem>> TiendasAsync() => Get<List<TiendaItem>>("api/tienda/publico/tiendas", false);

    public Task<List<UbigeoItem>> UbigeosAsync() => Get<List<UbigeoItem>>("api/tienda/publico/ubigeos", false);

    /* ------------------------------ cuenta ------------------------------ */

    public Task<Perfil> PerfilAsync() => Get<Perfil>("api/tienda/perfil");

    public Task<List<DireccionItem>> DireccionesAsync() => Get<List<DireccionItem>>("api/tienda/direcciones");

    public Task<DireccionItem> AgregarDireccionAsync(DireccionForm f) => Enviar<DireccionItem>(HttpMethod.Post, "api/tienda/direcciones", f);

    public Task<List<FacturacionItem>> FacturacionAsync() => Get<List<FacturacionItem>>("api/tienda/facturacion");

    public Task<FacturacionItem> AgregarFacturacionAsync(FacturacionForm f) => Enviar<FacturacionItem>(HttpMethod.Post, "api/tienda/facturacion", f);

    /* ------------------------------ carrito ------------------------------ */

    public Task<CarritoDto> CarritoAsync() => Get<CarritoDto>("api/tienda/carrito");

    public Task<CarritoDto> AgregarAlCarritoAsync(long productoId, int cantidad) =>
        ConCarrito(Enviar<CarritoDto>(HttpMethod.Post, "api/tienda/carrito/items", new { productoId, cantidad }));

    public Task<CarritoDto> ActualizarItemAsync(long id, int? cantidad = null, bool? seleccionado = null, string? estado = null) =>
        ConCarrito(Enviar<CarritoDto>(HttpMethod.Put, $"api/tienda/carrito/items/{id}", new { cantidad, seleccionado, estado }));

    public Task<CarritoDto> EliminarItemAsync(long id) => ConCarrito(Enviar<CarritoDto>(HttpMethod.Delete, $"api/tienda/carrito/items/{id}", null));

    private async Task<CarritoDto> ConCarrito(Task<CarritoDto> t)
    {
        var c = await t;
        sesion.ActualizarCarrito(c.CantidadActiva);
        return c;
    }

    /* ------------------------------ checkout y pedidos ------------------------------ */

    public Task<ResumenCheckout> ResumenAsync(CheckoutForm f) => Enviar<ResumenCheckout>(HttpMethod.Post, "api/tienda/checkout/resumen", f);

    public async Task<PedidoDetalle> ConfirmarPedidoAsync(CheckoutForm f)
    {
        var p = await Enviar<PedidoDetalle>(HttpMethod.Post, "api/tienda/checkout", f);
        var c = await CarritoAsync();
        sesion.ActualizarCarrito(c.CantidadActiva);
        return p;
    }

    public Task<PedidoDetalle> PagarAsync(long pedidoId, PagoRequest r) => Enviar<PedidoDetalle>(HttpMethod.Post, $"api/tienda/pedidos/{pedidoId}/pago", r);

    public Task<List<PedidoResumen>> PedidosAsync() => Get<List<PedidoResumen>>("api/tienda/pedidos");

    public Task<PedidoDetalle> PedidoAsync(long id) => Get<PedidoDetalle>($"api/tienda/pedidos/{id}");

    /* ------------------------------ infraestructura ------------------------------ */

    private Task<T> Get<T>(string url, bool auth = true) => Enviar<T>(HttpMethod.Get, url, null, auth);

    private async Task<T> Enviar<T>(HttpMethod metodo, string url, object? cuerpo, bool auth = true)
    {
        using var req = new HttpRequestMessage(metodo, url);
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, cuerpo.GetType(), options: Json);
        if (auth)
        {
            if (sesion.Token is null) throw new ApiException("Debes iniciar sesión", 401);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);
        }

        HttpResponseMessage resp;
        try
        {
            resp = await http.SendAsync(req);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("No se pudo conectar con el backend (" + http.BaseAddress + "). ¿Está levantado?", 0);
        }

        using (resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                return (await resp.Content.ReadFromJsonAsync<T>(Json))!;
            }
            var mensaje = await LeerError(resp);
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (auth)
                {
                    await sesion.CerrarAsync();
                    mensaje = "Tu sesión expiró. Vuelve a iniciar sesión.";
                }
                else if (url == "api/authenticate")
                {
                    mensaje = "Correo o contraseña incorrectos";
                }
            }
            throw new ApiException(mensaje, (int)resp.StatusCode);
        }
    }

    private static async Task<string> LeerError(HttpResponseMessage resp)
    {
        try
        {
            var doc = await resp.Content.ReadFromJsonAsync<JsonElement>(Json);
            if (doc.ValueKind == JsonValueKind.Object)
            {
                if (doc.TryGetProperty("fieldErrors", out var fe) && fe.ValueKind == JsonValueKind.Array && fe.GetArrayLength() > 0)
                {
                    return "Revisa los datos: " + string.Join(", ", fe.EnumerateArray().Select(e => $"{e.GetProperty("field").GetString()} ({e.GetProperty("message").GetString()})"));
                }
                foreach (var clave in new[] { "mensaje", "detail", "title" })
                {
                    if (doc.TryGetProperty(clave, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                        return v.GetString()!;
                }
            }
        }
        catch
        {
            // cuerpo vacio o no JSON
        }
        return $"Error {(int)resp.StatusCode} del servidor";
    }
}
