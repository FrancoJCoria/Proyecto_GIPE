using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Controllers;

// Ambos roles consultan el listado y la ficha; los cambios de maestro
// (altas, stock y receta) quedan reservados al Dueno.
[Authorize]

public class ProductosController : Controller
{
    private const int TamanoPagina = 10;

    private readonly IRepositorioProducto _repositorioProducto;
    private readonly IRepositorioInsumo _repositorioInsumo;
    private readonly ServicioCalculadora _servicioCalculadora;
    private readonly ServicioImagenes _servicioImagenes;

    public ProductosController(
        IRepositorioProducto repositorioProducto,
        IRepositorioInsumo repositorioInsumo,
        ServicioCalculadora servicioCalculadora,
        ServicioImagenes servicioImagenes)
    {
        _repositorioProducto = repositorioProducto;
        _repositorioInsumo = repositorioInsumo;
        _servicioCalculadora = servicioCalculadora;
        _servicioImagenes = servicioImagenes;
    }

    //------------------------------------------------------------LISTADO PAGINADO --------------------------------------------------------
    public IActionResult Index(int pagina = 1, string? busqueda = null)
    {
        var resultado = _repositorioProducto.ObtenerTodos(pagina, TamanoPagina, busqueda);

        ViewData["Busqueda"] = busqueda;
        AyudantePaginacion.CargarEnViewData(
            ViewData, pagina, TamanoPagina, resultado.Total, nameof(Index),
            rotas: new Dictionary<string, string?> { ["busqueda"] = busqueda });

        return View(resultado);
    }

    //------------------------------------------------------------ALTA --------------------------------------------------------
    [Authorize(Policy = "Dueno")]
    public IActionResult Crear()
    {
        return View(new Producto { MargenObjetivo = 30 });
    }

    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Producto producto, IFormFile? archivoProducto)
    {
        if (!ModelState.IsValid)
        {
            return View(producto);
        }

        producto.Nombre = producto.Nombre.Trim();

        int id = _repositorioProducto.CrearProducto(producto);

        if (id <= 0)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el producto.");
            return View(producto);
        }

        if (archivoProducto is not null && archivoProducto.Length > 0)
        {
            try
            {
                var urlNueva = _servicioImagenes.Guardar(archivoProducto, "Productos", null);

                if (urlNueva is not null)
                {
                    producto.Id = id;
                    producto.UrlProducto = urlNueva;
                    _repositorioProducto.ActualizarProducto(producto);
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"El producto se creo, pero no se pudo guardar la imagen: {ex.Message}";
            }
        }

        TempData["Exito"] = $"Producto '{producto.Nombre}' creado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    //------------------------------------------------------------EDICION (STOCK MANUAL) --------------------------------------------------------
    [Authorize(Policy = "Dueno")]
    public IActionResult Editar(int id)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        // Costo actual de los insumos de la receta para la calculadora en vivo.
        // Si el producto todavia no tiene receta es 0; el costo extra se aplica solo.
        var costoTotal = _repositorioProducto.ObtenerCostoProducto(id) ?? producto.CostoExtra;
        ViewBag.CostoInsumos = ServicioCalculadora.Redondear2(costoTotal - producto.CostoExtra);

        return View(producto);
    }

    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Producto producto, IFormFile? archivoProducto)
    {
        if (id != producto.Id)
        {
            ModelState.AddModelError(string.Empty, "El producto editado no coincide con el solicitado.");
            return View(producto);
        }

        if (!ModelState.IsValid)
        {
            return View(producto);
        }

        // La url vigente puede ser distinta a la que viene del form si van a subir
        // archivo; se necesita para borrar el archivo local anterior.
        var productoOriginal = _repositorioProducto.ObtenerPorId(id);

        if (productoOriginal is null)
        {
            return NoExiste();
        }

        var urlAnterior = productoOriginal.UrlProducto;

        if (archivoProducto is not null && archivoProducto.Length > 0)
        {
            try
            {
                // Guardar ya se encarga de borrar la imagen local anterior si la habia.
                var urlNueva = _servicioImagenes.Guardar(archivoProducto, "Productos", urlAnterior);

                if (urlNueva is not null)
                {
                    producto.UrlProducto = urlNueva;
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(producto.UrlProducto), ex.Message);
                return View(RecargarVistaEditar(producto));
            }
        }
        else if (producto.UrlProducto != urlAnterior)
        {
            // Se limpio o cambio la url sin subir archivo; si la vieja era local se borra.
            _servicioImagenes.BorrarSiLocal("Productos", urlAnterior);
        }

        producto.Nombre = producto.Nombre.Trim();

        if (!_repositorioProducto.ActualizarProducto(producto))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el producto.");
            return View(RecargarVistaEditar(producto));
        }

        TempData["Exito"] = $"Producto '{producto.Nombre}' actualizado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    // Recarga los datos de la vista de edicion cuando hay que volver a mostrarla.
    private IActionResult RecargarVistaEditar(Producto producto)
    {
        var costoTotal = _repositorioProducto.ObtenerCostoProducto(producto.Id) ?? producto.CostoExtra;
        ViewBag.CostoInsumos = ServicioCalculadora.Redondear2(costoTotal - producto.CostoExtra);
        return View(producto);
    }

    //------------------------------------------------------------DETALLES (FICHA + RECETA) --------------------------------------------------------
    public IActionResult Detalles(int id)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        return View(ArmarDetalle(producto));
    }

    //------------------------------------------------------------CARGA MANUAL DE STOCK --------------------------------------------------------
    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult AjustarStock(int id, decimal stock)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        if (stock < 0)
        {
            TempData["Error"] = "El stock no puede ser negativo.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        bool ajustado = _repositorioProducto.ActualizarStock(id, stock);

        TempData[ajustado ? "Exito" : "Error"] = ajustado
            ? $"Stock de '{producto.Nombre}' actualizado a {Formateos.Cantidad(stock)} unidades."
            : $"No se pudo actualizar el stock de '{producto.Nombre}'.";

        return RedirectToAction(nameof(Detalles), new { id });
    }

    //------------------------------------------------------------RECETA: AGREGAR / QUITAR INSUMO --------------------------------------------------------
    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult AgregarInsumo(int id, int insumoId, decimal cantidad)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad del insumo debe ser mayor a cero.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        var insumo = _repositorioInsumo.ObtenerPorId(insumoId);

        if (insumo is null || !insumo.Estado)
        {
            TempData["Error"] = "El insumo elegido no existe o esta inactivo.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // Se toma la receta actual, se reemplaza/suma la linea del insumo y se regraba completa.
        var lineas = _repositorioProducto.ObtenerReceta(id)
            .Select(r => new ProductoInsumo { ProductoId = id, InsumoId = r.InsumoId, Cantidad = r.Cantidad })
            .ToList();

        var linea = lineas.FirstOrDefault(l => l.InsumoId == insumoId);

        if (linea is null)
        {
            lineas.Add(new ProductoInsumo { ProductoId = id, InsumoId = insumoId, Cantidad = cantidad });
        }
        else
        {
            linea.Cantidad = cantidad;
        }

        bool guardado = _repositorioProducto.GuardarReceta(id, lineas);

        TempData[guardado ? "Exito" : "Error"] = guardado
            ? $"Insumo '{insumo.Nombre}' agregado a la receta de '{producto.Nombre}'."
            : $"No se pudo guardar la receta de '{producto.Nombre}'.";

        return RedirectToAction(nameof(Detalles), new { id });
    }

    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult QuitarInsumo(int id, int insumoId)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        bool quitado = _repositorioProducto.EliminarLineaReceta(id, insumoId);

        TempData[quitado ? "Exito" : "Error"] = quitado
            ? "Insumo quitado de la receta."
            : "El insumo no figura en la receta.";

        return RedirectToAction(nameof(Detalles), new { id });
    }

    //------------------------------------------------------------CALCULO JSON (PARA LA CALCULADORA Y PRUEBAS) --------------------------------------------------------
    // Devuelve costo, precio sugerido, margen real y ganancia del producto.
    // Opcionalmente acepta un margen distinto para previsualizar (no modifica datos).
    [HttpGet("Productos/CalculoJson/{id:int}/{margen?}")]
    [Authorize(Policy = "Dueno")]
    public IActionResult CalculoJson(int id, decimal? margen = null)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NotFound(new { mensaje = "El producto no existe." });
        }

        if (margen.HasValue && (margen < 0 || margen > 100))
        {
            ModelState.AddModelError("margen", "El margen debe estar entre 0 y 100.");
            return ValidationProblem();
        }

        var receta = _repositorioProducto.ObtenerReceta(id);
        var calculo = _servicioCalculadora.Calcular(receta, producto, margen);

        return Json(calculo);
    }

    //------------------------------------------------------------BUSQUEDA JSON (PARA SELECT2 EN VENTAS) --------------------------------------------------------    [HttpGet("Productos/BuscarJson")]
    public IActionResult BuscarJson(string term)
    {
        var productos = _repositorioProducto.ObtenerActivos();

        if (!string.IsNullOrWhiteSpace(term))
        {
            productos = productos
                .Where(p => p.Nombre.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var items = productos.Take(25).Select(p => new
        {
            id = p.Id,
            text = $"{p.Nombre} - {Formateos.Dinero(p.PrecioActual)} - stock: {Formateos.Cantidad(p.StockActual)}",
            precio = p.PrecioActual,
            stock = p.StockActual
        });

        return Json(new { items });
    }

    //------------------------------------------------------------BAJA LOGICA / REACTIVAR --------------------------------------------------------
    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult DarDeBaja(int id)
    {
        return CambiarEstado(id, false, "dado de baja");
    }

    [HttpPost]
    [Authorize(Policy = "Dueno")]
    [ValidateAntiForgeryToken]
    public IActionResult Activar(int id)
    {
        return CambiarEstado(id, true, "reactivado");
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private ProductoDetalleViewModel ArmarDetalle(Producto producto)
    {
        var receta = _repositorioProducto.ObtenerReceta(producto.Id);
        var calculo = _servicioCalculadora.Calcular(receta, producto);

        return new ProductoDetalleViewModel
        {
            Producto = producto,
            Receta = receta,
            CostoTotal = calculo.CostoTotal,
            PrecioSugerido = calculo.PrecioSugerido,
            MargenActual = calculo.MargenActual,
            GananciaUnitaria = calculo.GananciaUnitaria,
            InsumosDisponibles = _repositorioInsumo.ObtenerActivos()
        };
    }

    private IActionResult CambiarEstado(int id, bool nuevoEstado, string accion)
    {
        var producto = _repositorioProducto.ObtenerPorId(id);

        if (producto is null)
        {
            return NoExiste();
        }

        bool cambiado = _repositorioProducto.CambiarEstadoProducto(id, nuevoEstado);

        TempData[cambiado ? "Exito" : "Error"] = cambiado
            ? $"Producto '{producto.Nombre}' fue {accion}."
            : $"No se pudo cambiar el estado del producto '{producto.Nombre}'.";

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NoExiste()
    {
        TempData["Error"] = "El producto solicitado no existe.";
        return RedirectToAction(nameof(Index));
    }
}
