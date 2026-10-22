using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class ProductoInsumo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Required]
    public int InsumoId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }
}
