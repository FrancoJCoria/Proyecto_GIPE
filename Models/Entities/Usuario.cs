using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.Entities;

public class Usuario
{
    [Key]
    public int Id {get; set;}

    [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre supera los 150 caracteres."), MinLength(6, ErrorMessage = "El nombre debe contener 6 o mas caracteres.")]
    public string Nombre {get; set;} = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    public string? Url_avatar {get; set;} = string.Empty;

    [Required]
    public bool Estado { get; set; } = true;
}

