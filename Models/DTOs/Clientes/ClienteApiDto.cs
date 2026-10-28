using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.DTOs.Clientes;

/// <summary>Datos que acepta la API de clientes (alta y edicion).</summary>
public sealed class ClienteApiDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Apellido { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }
}

public sealed class ClienteEstadoApiDto
{
    public bool Estado { get; set; }
}
