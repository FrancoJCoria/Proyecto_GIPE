using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La descripcion no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0, 999999999, ErrorMessage = "El costo extra no puede ser negativo.")]
    public decimal CostoExtra { get; set; }

    [Required(ErrorMessage = "El margen objetivo es obligatorio.")]
    [Range(0, 100, ErrorMessage = "El margen objetivo debe estar entre 0 y 100.")]
    public decimal MargenObjetivo { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0, 999999999, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal PrecioActual { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 999999999, ErrorMessage = "El stock no puede ser negativo.")]
    public decimal StockActual { get; set; }

    public string? UrlProducto { get; set; }

    public bool Estado { get; set; } = true;
}
