using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.DTOs.Productos;

public sealed class ProductoApiDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal CostoExtra { get; set; }

    [Range(typeof(decimal), "0", "1000")]
    public decimal MargenObjetivo { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PrecioActual { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal StockActual { get; set; }

    [StringLength(500)]
    public string? UrlProducto { get; set; }
}

public sealed class AjustarStockApiDto
{
    [Range(typeof(decimal), "0", "999999999")]
    public decimal Stock { get; set; }
}
