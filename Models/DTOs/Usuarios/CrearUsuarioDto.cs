using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.DTOs.Usuarios;

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(6)]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;



    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;


    [Required]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(500)]
    [Url(ErrorMessage = "La URL del avatar no es valida :p")] 
    public string? Url_avatar{get; set;}


    [Required]
    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;
}