using System.Globalization;

namespace TiendaTech.Services;

/// <summary>Formatos de moneda, fecha y textos de estados.</summary>
public static class Formato
{
    private static readonly CultureInfo Pe = CultureInfo.GetCultureInfo("es-PE");
    private static readonly TimeZoneInfo Lima = BuscarZona();

    private static TimeZoneInfo BuscarZona()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Lima"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); }
    }

    public static string Soles(decimal? v) => v is null ? "-" : "S/ " + v.Value.ToString("N2", Pe);

    public static DateTime HoraLima(DateTimeOffset f) => TimeZoneInfo.ConvertTime(f, Lima).DateTime;

    public static string Fecha(DateTimeOffset? f) => f is null ? "-" : HoraLima(f.Value).ToString("dd/MM/yyyy HH:mm", Pe);

    public static string Dia(DateOnly? f) => f is null ? "-" : f.Value.ToString("dddd dd 'de' MMMM", Pe);

    public static int PorcentajeDescuento(decimal lista, decimal? oferta) =>
        oferta is null || lista <= 0 ? 0 : (int)Math.Round((lista - oferta.Value) / lista * 100);

    public static string Estado(string e) => e switch
    {
        "PENDIENTE_PAGO" => "Pendiente de pago",
        "PAGADO" => "Pagado",
        "EN_PREPARACION" => "En preparación",
        "ENVIADO" => "En camino",
        "LISTO_RECOJO" => "Listo para recoger",
        "ENTREGADO" => "Entregado",
        "RECHAZADO" => "Rechazado",
        "ANULADO" => "Anulado",
        "CANCELADO" => "Cancelado",
        "EXPIRADO" => "Expirado",
        "APROBADO" => "Aprobado",
        "REEMBOLSADO" => "Reembolsado",
        "PENDIENTE" => "Pendiente",
        "EN_RUTA" => "En ruta",
        "DEVUELTO" => "Devuelto",
        "ACEPTADO" => "Aceptado por SUNAT",
        "OBSERVADO" => "Observado",
        _ => e
    };

    public static string ColorEstado(string e) => e switch
    {
        "PENDIENTE_PAGO" or "PENDIENTE" => "warning",
        "PAGADO" or "EN_PREPARACION" or "APROBADO" => "info",
        "ENVIADO" or "LISTO_RECOJO" or "EN_RUTA" => "primary",
        "ENTREGADO" or "ACEPTADO" => "success",
        "EXPIRADO" or "REEMBOLSADO" or "DEVUELTO" => "secondary",
        _ => "danger"
    };

    public static string Entrega(string t) => t == "DELIVERY" ? "Delivery a domicilio" : "Recojo en tienda";

    public static string Metodo(string m, string? sub) => (m switch
    {
        "TARJETA_CREDITO" => "Tarjeta de crédito",
        "TARJETA_DEBITO" => "Tarjeta de débito",
        "BILLETERA_DIGITAL" => "Billetera digital",
        "TRANSFERENCIA" => "Transferencia",
        "EFECTIVO" => "Efectivo",
        _ => m
    }) + (string.IsNullOrEmpty(sub) ? "" : $" ({sub})");
}
