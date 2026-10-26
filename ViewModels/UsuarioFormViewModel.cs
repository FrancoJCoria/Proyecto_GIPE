using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.ViewModels;

public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(6, ErrorMessage = "El nombre debe tener al menos 6 caracteres.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
    [MaxLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [EnumDataType(typeof(RolUsuario), ErrorMessage = "Seleccioná un rol válido.")]
    [Display(Name = "Rol")]
    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    [MaxLength(500, ErrorMessage = "La URL del avatar no puede superar los 500 caracteres.")]
    [Display(Name = "URL del avatar")]
    public string? Url_avatar { get; set; }

    [Display(Name = "Activo")]
    public bool Estado { get; set; } = true;
}
