using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.DTOs.Usuarios;

public class ActualizarUsuarioDto
{
    [Required(ErrorMessage ="El nombre es obligatorio")]
    [MinLength(6, ErrorMessage = "El nombre debe tener al menos 6 caracteres")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage ="El correo no tiene un formato valido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public RolUsuario? Rol { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "La URL del avatar no es valida.")]
    public string? Url_avatar{get; set;}

    public bool Estado { get; set; }
}
