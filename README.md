# TiendaTech (front Blazor .NET 10)

Tienda web para el cliente de commerce-app: catálogo, detalle, registro/login, carrito,
checkout (pedido PENDIENTE_PAGO con 30 minutos para pagar), pago simulado y mis pedidos.

## Requisitos
- .NET SDK 10
- Backend commerce-app corriendo en http://localhost:9999 (con el paquete `pe.commerce.app.tienda`)

## Ejecutar
```
dotnet run
```
Abre http://localhost:5080

La URL del backend se cambia en `appsettings.json` → `Api:BaseUrl`.

## Estructura
- `Services/ApiClient.cs` — una función por endpoint de `/api/tienda/**`
- `Services/Sesion.cs` — token JWT (guardado cifrado en el navegador) y contador del carrito
- `Components/Pages` — páginas
- `Components/Shared` — componentes reutilizables
- `wwwroot/Image/Productos` — imágenes referenciadas por el catálogo del seed
