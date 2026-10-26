using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Modelo de la vista Detalles de una venta: cabezal + items con precio/costo congelados.</summary>
public class VentaDetalleViewModel
{
    public Venta Venta { get; set; } = null!;

    public string? ClienteNombre { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public IList<ItemVentaDetalle> Items { get; set; } = [];

    /// <summary>Costo de produccion acumulado de los items vendidos.</summary>
    public decimal TotalCosto => Items.Sum(i => i.SubtotalCosto);

    /// <summary>Ganancia de la venta: monto total menos costo total.</summary>
    public decimal Ganancia => Venta.MontoTotal - TotalCosto;
}

/// <summary>Un item vendido con los valores guardados al momento de la venta.</summary>
public class ItemVentaDetalle
{
    public int ProductoId { get; set; }

    public string ProductoNombre { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    /// <summary>Precio unitario del producto al momento de la venta.</summary>
    public decimal PrecioMomento { get; set; }

    /// <summary>Costo unitario (receta + costo extra) al momento de la venta.</summary>
    public decimal CostoMomento { get; set; }

    public decimal Subtotal => PrecioMomento * Cantidad;

    public decimal SubtotalCosto => CostoMomento * Cantidad;
}
