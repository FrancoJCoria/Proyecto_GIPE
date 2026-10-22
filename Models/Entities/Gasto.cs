using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.Entities;

public class Gasto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [MaxLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Otro;

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;
}
