using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.Entities;

public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Apellido { get; set; }

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    public bool Estado { get; set; } = true;
}
