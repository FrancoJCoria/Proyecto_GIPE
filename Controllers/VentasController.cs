using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Excepciones;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Controllers;

[Authorize]
public class VentasController : Controller
{
    private const int TamanoPagina = 10;

    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IRepositorioCliente _repositorioCliente;
    private readonly IRepositorioProducto _repositorioProducto;
    private readonly IServicioUsuarioActual _servicioUsuarioActual;

    public VentasController(
        IRepositorioVenta repositorioVenta,
        IRepositorioCliente repositorioCliente,
        IRepositorioProducto repositorioProducto,
        IServicioUsuarioActual servicioUsuarioActual)
    {
        _repositorioVenta = repositorioVenta;
        _repositorioCliente = repositorioCliente;
        _repositorioProducto = repositorioProducto;
        _servicioUsuarioActual = servicioUsuarioActual;
    }

    //------------------------------------------------------------LISTADO PAGINADO --------------------------------------------------------
    // El Dueno ve todas las ventas; el Empleado solo las que registro.
    public IActionResult Index(int pagina = 1)
    {
        int? usuarioId = User.IsInRole("Dueno") ? null : _servicioUsuarioActual.ObtenerUsuarioId();

        var resultado = _repositorioVenta.ObtenerTodos(pagina, TamanoPagina, usuarioId);

        AyudantePaginacion.CargarEnViewData(ViewData, pagina, TamanoPagina, resultado.Total, nameof(Index));

        return View(resultado);
    }

    //------------------------------------------------------------ALTA (ITEMS CON SELECT2) --------------------------------------------------------
    public IActionResult Crear()
    {
        var modelo = new VentaCrearViewModel();
        CargarSelects(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(VentaCrearViewModel modelo)
    {
        modelo ??= new VentaCrearViewModel();

        // Se arman las lineas de venta desde el formulario:
        // - las filas sin producto elegido se ignoran (fila vacia que quedo en pantalla);
        // - las filas con producto deben tener cantidad mayor a cero.
        var lineas = new List<LineaVenta>();

        foreach (var item in modelo.Items)
        {
            if (item is null || item.ProductoId is null || item.ProductoId <= 0)
            {
                continue;
            }

            if (item.Cantidad is null || item.Cantidad <= 0)
            {
                ModelState.AddModelError(string.Empty, "Todas las cantidades deben ser mayores a cero.");
                break;
            }

            lineas.Add(new LineaVenta(item.ProductoId.Value, item.Cantidad.Value));
        }

        if (lineas.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Agregá al menos un item a la venta.");
        }

        if (!ModelState.IsValid)
        {
            CargarSelects(modelo);
            return View(modelo);
        }

        try
        {
            // El id viene del claim del usuario con sesion activa ([Authorize] en el controller).
            int usuarioId = _servicioUsuarioActual.ObtenerUsuarioId();

            int ventaId = _repositorioVenta.CrearVenta(modelo.ClienteId, usuarioId, lineas);

            TempData["Exito"] = $"Venta #{ventaId} registrada correctamente.";
            return RedirectToAction(nameof(Detalles), new { id = ventaId });
        }
        catch (ReglaDeNegocioException e)
        {
            // Producto/cliente inexistente o stock insuficiente: se muestran los items cargados
            // para que el usuario corrija sin perder el formulario.
            ModelState.AddModelError(string.Empty, e.Message);
            CargarSelects(modelo);
            return View(modelo);
        }
    }

    //------------------------------------------------------------DETALLE --------------------------------------------------------
    public IActionResult Detalles(int id)
    {
        var detalle = _repositorioVenta.ObtenerDetalle(id);

        if (detalle is null)
        {
            TempData["Error"] = "La venta solicitada no existe.";
            return RedirectToAction(nameof(Index));
        }

        // El Empleado no puede ver ventas de otros usuarios.
        if (!User.IsInRole("Dueno") && detalle.Venta.UsuarioId != _servicioUsuarioActual.ObtenerUsuarioId())
        {
            TempData["Error"] = "No tenés permiso para ver esta venta.";
            return RedirectToAction(nameof(Index));
        }

        return View(detalle);
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private void CargarSelects(VentaCrearViewModel modelo)
    {
        ViewBag.Clientes = _repositorioCliente.ObtenerActivos();
        ViewBag.Productos = _repositorioProducto.ObtenerActivos();
    }
}
