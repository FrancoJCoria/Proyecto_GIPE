using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Modelo de la vista Detalles de un producto: ficha, receta e insumos disponibles para agregar.</summary>
public class ProductoDetalleViewModel
{
    public Producto Producto { get; set; } = null!;

    /// <summary>Lineas de la receta (producto_insumo) para este producto.</summary>
    public IList<RecetaItem> Receta { get; set; } = [];

    /// <summary>Costo de produccion calculado: costo de los insumos de la receta + costo extra.</summary>
    public decimal CostoTotal { get; set; }

    /// <summary>Costo extra definido en el producto (fletes, coccion, etc.), ya incluido en CostoTotal.</summary>
    public decimal CostoExtra => Producto.CostoExtra;

    /// <summary>Costo de solo los insumos de la receta, sin el costo extra.</summary>
    public decimal CostoInsumos => CostoTotal - CostoExtra;

    /// <summary>Precio sugerido: costo total con el margen objetivo aplicado.</summary>
    public decimal PrecioSugerido { get; set; }

    /// <summary>Margen real del precio vigente sobre el costo total (%).</summary>
    public decimal MargenActual { get; set; }

    /// <summary>Ganancia unitaria: precio vigente menos costo total.</summary>
    public decimal GananciaUnitaria { get; set; }
}
