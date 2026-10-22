using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.DTOs.Usuarios;

public class UsuarioRespuestaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Estado { get; set; }
    public string? Url_avatar { get; set; }
}