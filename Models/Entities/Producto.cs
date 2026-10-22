using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public decimal CostoExtra { get; set; }

    [Required]
    public decimal MargenObjetivo { get; set; }

    [Required]
    public decimal PrecioActual { get; set; }

    [Required]
    public decimal StockActual { get; set; }

    public string? UrlProducto { get; set; }

    public bool Estado { get; set; } = true;
}
