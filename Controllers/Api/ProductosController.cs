using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Productos;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/productos")]
public sealed class ProductosApiController : ControllerBase
{
    private const int TamanoMaximoPagina = 100;
    private readonly IRepositorioProducto _productos;

    public ProductosApiController(IRepositorioProducto productos) => _productos = productos;

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult ObtenerTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] string? busqueda = null,
        [FromQuery] int tamano = 20,
        [FromQuery] bool soloActivos = false)
    {
        if (pagina < 1) return BadRequest(new { mensaje = "La página debe ser mayor a cero." });
        // Tamano acotado: la busqueda asincrona de formularios pide paginas chicas.
        tamano = Math.Clamp(tamano, 1, TamanoMaximoPagina);
        var resultado = _productos.ObtenerTodos(pagina, tamano, busqueda, soloActivos);
        return Ok(new { pagina, tamano, total = resultado.Total, productos = resultado.Filas });
    }

    [HttpGet("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Empleado")]
    public IActionResult ObtenerPorId(int id)
    {
        var producto = _productos.ObtenerPorId(id);
        return producto is null ? NotFound(new { mensaje = "El producto no existe." }) : Ok(producto);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult Crear([FromBody] ProductoApiDto dto)
    {
        var producto = Mapear(dto);
        var id = _productos.CrearProducto(producto);
        if (id <= 0) return StatusCode(500, new { mensaje = "No se pudo crear el producto." });
        producto.Id = id;
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, producto);
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult Actualizar(int id, [FromBody] ProductoApiDto dto)
    {
        var existente = _productos.ObtenerPorId(id);
        if (existente is null) return NotFound(new { mensaje = "El producto no existe." });
        var producto = Mapear(dto);
        producto.Id = id;
        producto.Estado = existente.Estado;
        return _productos.ActualizarProducto(producto) ? NoContent() : StatusCode(500, new { mensaje = "No se pudo actualizar el producto." });
    }

    [HttpPatch("{id:int}/stock")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult ActualizarStock(int id, [FromBody] AjustarStockApiDto dto)
    {
        if (_productos.ObtenerPorId(id) is null) return NotFound(new { mensaje = "El producto no existe." });
        return _productos.ActualizarStock(id, dto.Stock) ? NoContent() : StatusCode(500, new { mensaje = "No se pudo actualizar el stock." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Dueno")]
    public IActionResult DarDeBaja(int id)
    {
        if (_productos.ObtenerPorId(id) is null) return NotFound(new { mensaje = "El producto no existe." });
        return _productos.CambiarEstadoProducto(id, false) ? NoContent() : StatusCode(500, new { mensaje = "No se pudo dar de baja el producto." });
    }

    private static Producto Mapear(ProductoApiDto dto) => new()
    {
        Nombre = dto.Nombre.Trim(), Descripcion = dto.Descripcion?.Trim(), CostoExtra = dto.CostoExtra,
        MargenObjetivo = dto.MargenObjetivo, PrecioActual = dto.PrecioActual, StockActual = dto.StockActual,
        UrlProducto = dto.UrlProducto?.Trim(), Estado = true
    };
}
