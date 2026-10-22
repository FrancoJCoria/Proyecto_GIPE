using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class VentaItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VentaId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    public decimal PrecioMomento { get; set; }

    public decimal CostoMomento { get; set; }
}
