using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesApiController : ControllerBase
{
    private const int TamanoMaximoPagina = 25;
    private readonly IRepositorioCliente _clientes;

    public ClientesApiController(IRepositorioCliente clientes) => _clientes = clientes;

    // Busqueda paginada para los desplegables asincronos (Select2 en Nueva venta).
    // Con esquemas Cookie + JWT: la web la consume con su sesion y la API con token.
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult Buscar(
        [FromQuery] int pagina = 1,
        [FromQuery] string? busqueda = null,
        [FromQuery] int tamano = TamanoMaximoPagina)
    {
        if (pagina < 1) return BadRequest(new { mensaje = "La página debe ser mayor a cero." });
        tamano = Math.Clamp(tamano, 1, TamanoMaximoPagina);

        // Solo activos: un cliente dado de baja no debe poder asociarse a una venta.
        var resultado = _clientes.ObtenerTodos(pagina, tamano, busqueda, soloActivos: true);

        var clientes = resultado.Filas.Select(c => new
        {
            id = c.Id,
            texto = string.IsNullOrWhiteSpace(c.Apellido) ? c.Nombre : $"{c.Nombre} {c.Apellido}",
            telefono = c.Telefono,
            email = c.Email
        });

        return Ok(new { pagina, tamano, total = resultado.Total, clientes });
    }
}
