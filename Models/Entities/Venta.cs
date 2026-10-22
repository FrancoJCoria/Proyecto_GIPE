using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class Venta
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int? ClienteId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    public decimal MontoTotal { get; set; }

    public bool Estado { get; set; } = true;
}
