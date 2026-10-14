using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.DTOs.Usuarios;

public class ActualizarUsuarioDto
{
    [Required]
    [MinLength(6)]
    [MaxLength(20)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public int Rol { get; set; }

    public bool Estado { get; set; }
}