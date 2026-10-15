
using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.DTOs.Usuarios;

public class LoginUsuarioDto
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo invalido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}
