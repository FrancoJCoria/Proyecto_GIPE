namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Linea que compone el alta de una venta nueva.
/// </summary>
/// <param name="ProductoId">Id del producto vendido.</param>
/// <param name="Cantidad">Cantidad de unidades vendidas.</param>
public record LineaVenta(int ProductoId, decimal Cantidad);
