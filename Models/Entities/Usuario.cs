using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.Entities;

public class Usuario
{
    [Key]
    public int Id {get; set;}

    [Required]
    [MaxLength(20), MinLength(6)]
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

    [Required]
    public bool Estado { get; set; } = true;
}

