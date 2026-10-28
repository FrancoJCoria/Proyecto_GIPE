using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Clientes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

/// <summary>
/// Web API de clientes protegida con JWT. Es la capa que consume el ABM
/// hecho en Vue (Clientes/Vue): consulta, alta, edicion y baja logica.
/// </summary>
[ApiController]
[Route("api/clientes")]
public sealed class ClientesApiController : ControllerBase
{
    private const int TamanoMaximoPagina = 100;
    private readonly IRepositorioCliente _clientes;

    public ClientesApiController(IRepositorioCliente clientes) => _clientes = clientes;

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult ObtenerTodos([FromQuery] int pagina = 1, [FromQuery] string? busqueda = null)
    {
        if (pagina < 1) return BadRequest(new { mensaje = "La página debe ser mayor a cero." });
        var resultado = _clientes.ObtenerTodos(pagina, TamanoMaximoPagina, busqueda);
        return Ok(new { pagina, tamano = TamanoMaximoPagina, total = resultado.Total, clientes = resultado.Filas });
    }

    [HttpGet("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult ObtenerPorId(int id)
    {
        var cliente = _clientes.ObtenerPorId(id);
        return cliente is null ? NotFound(new { mensaje = "El cliente no existe." }) : Ok(cliente);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult Crear([FromBody] ClienteApiDto dto)
    {
        var cliente = Mapear(dto);
        var id = _clientes.CrearCliente(cliente);
        if (id <= 0) return StatusCode(500, new { mensaje = "No se pudo crear el cliente." });
        cliente.Id = id;
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, cliente);
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult Actualizar(int id, [FromBody] ClienteApiDto dto)
    {
        var existente = _clientes.ObtenerPorId(id);
        if (existente is null) return NotFound(new { mensaje = "El cliente no existe." });
        var cliente = Mapear(dto);
        cliente.Id = id;
        cliente.Estado = existente.Estado;
        return _clientes.ActualizarCliente(cliente) ? NoContent() : StatusCode(500, new { mensaje = "No se pudo actualizar el cliente." });
    }

    [HttpPatch("{id:int}/estado")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult CambiarEstado(int id, [FromBody] ClienteEstadoApiDto dto)
    {
        if (_clientes.ObtenerPorId(id) is null) return NotFound(new { mensaje = "El cliente no existe." });
        return _clientes.CambiarEstadoCliente(id, dto.Estado)
            ? NoContent()
            : StatusCode(500, new { mensaje = "No se pudo cambiar el estado del cliente." });
    }

    private static Cliente Mapear(ClienteApiDto dto) => new()
    {
        Nombre = dto.Nombre.Trim(),
        Apellido = dto.Apellido?.Trim(),
        Telefono = dto.Telefono?.Trim(),
        Email = dto.Email?.Trim(),
        Direccion = dto.Direccion?.Trim(),
        Estado = true
    };
}
