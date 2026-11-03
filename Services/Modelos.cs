using System.ComponentModel.DataAnnotations;

namespace TiendaTech.Services;

/* ------------------------------ catalogo ------------------------------ */

public record CategoriaNodo(long Id, string Nombre, string Slug, List<CategoriaNodo>? Hijas);

public record MarcaItem(long Id, string Nombre);

public record ProductoCard(
    long Id, string Sku, string Nombre, string Slug, string Marca, string Categoria,
    decimal PrecioLista, decimal? PrecioOferta, string? Imagen, double? Rating, long Resenas);

public record Pagina<T>(List<T> Items, long Total, int Page, int Size)
{
    public int TotalPaginas => Size == 0 ? 0 : (int)Math.Ceiling(Total / (double)Size);
}

public record StockTienda(long TiendaId, string Tienda, int Disponible);

public record ResenaItem(string Cliente, int Calificacion, string? Comentario, DateTimeOffset Fecha, bool CompraVerificada);

public record ProductoDetalle(
    long Id, string Sku, string Nombre, string Slug, string? Modelo, string Marca, string Categoria, string CategoriaSlug,
    string? Descripcion, string? Especificaciones, decimal PrecioLista, decimal? PrecioOferta, int? GarantiaMeses,
    List<string> Imagenes, int StockOnline, List<StockTienda> Tiendas, double? Rating, List<ResenaItem> Resenas);

public record TiendaItem(long Id, string Nombre, string Direccion, string Distrito);

public record UbigeoItem(long Id, string Codigo, string Departamento, string Provincia, string Distrito)
{
    public string Texto => $"{Distrito} - {Provincia} ({Departamento})";
}

/* ------------------------------ cuenta ------------------------------ */

public record Perfil(
    long ClienteId, string Codigo, string Nombres, string Apellidos, string Email,
    string? TipoDocumento, string? NumeroDocumento, string? Celular);

public record DireccionItem(long Id, string Alias, string Direccion, string? Referencia, long UbigeoId, string Ubigeo, bool Predeterminada);

public record FacturacionItem(long Id, string Ruc, string RazonSocial, string DireccionFiscal, bool Predeterminado);

public record Cuenta(string Login, string? FirstName, string? LastName, string? Email, List<string>? Authorities);

/* ------------------------------ carrito ------------------------------ */

public record CarritoLinea(
    long Id, long ProductoId, string Sku, string Nombre, string Slug, string? Imagen,
    decimal PrecioLista, decimal? PrecioOferta, decimal Precio, int Cantidad, bool Seleccionado,
    string Estado, int StockDisponible, decimal Subtotal);

public record CarritoDto(List<CarritoLinea> Items, int CantidadSeleccionada, decimal SubtotalSeleccionado)
{
    public int CantidadActiva => Items.Where(i => i.Estado == "ACTIVO").Sum(i => i.Cantidad);
}

/* ------------------------------ checkout y pedidos ------------------------------ */

public record LineaResumen(string Sku, string Nombre, int Cantidad, decimal Precio, decimal Subtotal, string TipoPrecio);

public record ResumenCheckout(
    List<LineaResumen> Lineas, decimal Subtotal, decimal Descuento, string? Cupon, string? MensajeCupon,
    decimal CostoEnvio, decimal Igv, decimal Total);

public record PedidoResumen(
    long Id, string NroPedido, string Estado, string TipoEntrega, DateTimeOffset FechaPedido,
    DateTimeOffset FechaExpiracion, decimal Total, long Items);

public record LineaPedido(
    string Sku, string Nombre, string Slug, string? Imagen, int Cantidad, string TipoPrecio,
    decimal PrecioUnitario, decimal Descuento, decimal Subtotal);

public record EstadoItem(string Estado, string? Comentario, DateTimeOffset? Fecha);

public record PagoItem(string Metodo, string? SubMetodo, string Estado, decimal Monto, DateTimeOffset Fecha, string? NroTransaccion);

public record ComprobanteItem(string Tipo, string Serie, int Numero, string EstadoSunat, decimal Total, DateTimeOffset Fecha, string Receptor);

public record EnvioItem(string? Courier, string? Tracking, string Estado, string Direccion, DateOnly? FechaEstimada);

public record PedidoDetalle(
    long Id, string NroPedido, string Estado, string TipoEntrega, string TipoComprobante,
    DateTimeOffset FechaPedido, DateTimeOffset FechaExpiracion,
    decimal Subtotal, decimal Descuento, decimal CostoEnvio, decimal Igv, decimal Total,
    string? Cupon, string? TiendaRecojo, string? Observaciones,
    List<LineaPedido> Lineas, List<EstadoItem> Historial, List<PagoItem> Pagos,
    List<ComprobanteItem> Comprobantes, EnvioItem? Envio);

/* ------------------------------ formularios (requests) ------------------------------ */

public class LoginForm
{
    [Required(ErrorMessage = "Ingresa tu correo"), EmailAddress(ErrorMessage = "Correo no válido")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; } = true;
}

public class RegistroForm
{
    [Required(ErrorMessage = "Ingresa tus nombres"), StringLength(100)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tus apellidos"), StringLength(100)]
    public string Apellidos { get; set; } = "";

    [Required] public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "Ingresa tu número de documento"), StringLength(15)]
    public string NumeroDocumento { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu celular"), RegularExpression(@"^9\d{8}$", ErrorMessage = "9 dígitos, empieza con 9")]
    public string Celular { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu correo"), EmailAddress(ErrorMessage = "Correo no válido"), StringLength(50)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresa una contraseña"), MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = "";

    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string Confirmacion { get; set; } = "";

    public bool AceptaPublicidad { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos")]
    public bool AceptaTerminos { get; set; }
}

public class DireccionForm
{
    [Required(ErrorMessage = "Ponle un nombre (Casa, Oficina...)"), StringLength(30)]
    public string Alias { get; set; } = "Casa";

    [Required(ErrorMessage = "Ingresa la dirección"), StringLength(200)]
    public string Direccion { get; set; } = "";

    [StringLength(200)] public string? Referencia { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "Elige el distrito")]
    public long UbigeoId { get; set; }

    public bool Predeterminada { get; set; }
}

public class FacturacionForm
{
    [Required(ErrorMessage = "Ingresa el RUC"), RegularExpression(@"^(10|20)\d{9}$", ErrorMessage = "11 dígitos, empieza con 10 o 20")]
    public string Ruc { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la razón social"), StringLength(200)]
    public string RazonSocial { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la dirección fiscal"), StringLength(250)]
    public string DireccionFiscal { get; set; } = "";

    public bool Predeterminado { get; set; }
}

public class CheckoutForm
{
    public string TipoEntrega { get; set; } = "DELIVERY";
    public long? DireccionId { get; set; }
    public long? TiendaId { get; set; }
    public string TipoComprobante { get; set; } = "BOLETA";
    public long? DatoFacturacionId { get; set; }
    public string? CuponCodigo { get; set; }
    public string? Observaciones { get; set; }
}

public record PagoRequest(string Metodo, string? SubMetodo, long? DatoFacturacionId);
