using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.DTOs.Ventas;
using Proyecto_GIPE.Models.Excepciones;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/ventas")]
public sealed class VentasApiController : ControllerBase
{
    private const int TamanoPagina = 50;
    private readonly IRepositorioVenta _ventas;

    public VentasApiController(IRepositorioVenta ventas) => _ventas = ventas;

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme,Policy = "Empleado")]
    public IActionResult ObtenerTodos([FromQuery] int pagina = 1)
    {
        if (pagina < 1) return BadRequest(new { mensaje = "La pagina debe ser mayor" });
        if (!ObtenerUsuarioId(out var usuarioId)) return Unauthorized(new { mensaje = "el token no es valido" });
        int? filtroUsuario = User.IsInRole("Dueno") ? null : usuarioId;
        var resultado = _ventas.ObtenerTodos(pagina, TamanoPagina, filtroUsuario);
        return Ok(new { pagina, tamano = TamanoPagina, total = resultado.Total, ventas = resultado.Filas });
    }

    [HttpGet("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme,Policy = "Empleado")]
    public IActionResult ObtenerPorId(int id)
    {
        if (!ObtenerUsuarioId(out var usuarioId)) return Unauthorized(new { mensaje = "Token inválido." });
        var detalle = _ventas.ObtenerDetalle(id);
        if (detalle is null) return NotFound(new { mensaje = "Esta venta no existe" });
        if (!User.IsInRole("Dueno") && detalle.Venta.UsuarioId != usuarioId)
            return Forbid(JwtBearerDefaults.AuthenticationScheme);
        return Ok(detalle);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult Crear([FromBody] CrearVentaApiDto dto)
    {
        if (!ObtenerUsuarioId(out var usuarioId)) return Unauthorized(new { mensaje = "Token inválido." });
        try
        {
            var lineas = dto.Items.Select(i => new LineaVenta(i.ProductoId, i.Cantidad)).ToList();
            var ventaId = _ventas.CrearVenta(dto.ClienteId, usuarioId, lineas);
            var detalle = _ventas.ObtenerDetalle(ventaId);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = ventaId }, detalle);
        }
        catch (ReglaDeNegocioException e)
        {
            return Conflict(new { mensaje = e.Message });
        }
    }

    private bool ObtenerUsuarioId(out int id)
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id > 0;
    }
}
