using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/insumos")]
public sealed class InsumosApiController : ControllerBase
{
    private const int TamanoMaximoPagina = 25;
    private readonly IRepositorioInsumo _insumos;

    public InsumosApiController(IRepositorioInsumo insumos) => _insumos = insumos;

    // Busqueda paginada para el desplegable asincrono de la receta del producto.
    // La receta solo la gestiona el Dueno, la busqueda queda en la misma policy.
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult Buscar(
        [FromQuery] int pagina = 1,
        [FromQuery] string? busqueda = null,
        [FromQuery] int tamano = TamanoMaximoPagina)
    {
        if (pagina < 1) return BadRequest(new { mensaje = "La página debe ser mayor a cero." });
        tamano = Math.Clamp(tamano, 1, TamanoMaximoPagina);

        // Solo activos: un insumo de baja no debe agregarse a una receta.
        var resultado = _insumos.ObtenerTodos(pagina, tamano, busqueda, soloActivos: true);

        var insumos = resultado.Filas.Select(i => new
        {
            id = i.Id,
            texto = $"{i.Nombre} ({i.Unidad} - $ {i.CostoUnitario:0.##})",
            unidad = i.Unidad
        });

        return Ok(new { pagina, tamano, total = resultado.Total, insumos });
    }
}
