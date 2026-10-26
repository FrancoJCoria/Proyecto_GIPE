using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Controllers;

// Tanto el Dueno como el Empleado gestionan clientes (ambos tienen vista completa).
[Authorize]
public class ClientesController : Controller
{
    private const int TamanoPagina = 10;
    private const int TamanoPaginaHistorial = 5;

    private readonly IRepositorioCliente _repositorioCliente;

    public ClientesController(IRepositorioCliente repositorioCliente)
    {
        _repositorioCliente = repositorioCliente;
    }

    //------------------------------------------------------------LISTADO PAGINADO --------------------------------------------------------
    public IActionResult Index(int pagina = 1, string? busqueda = null)
    {
        var resultado = _repositorioCliente.ObtenerTodos(pagina, TamanoPagina, busqueda);

        ViewData["Busqueda"] = busqueda;
        AyudantePaginacion.CargarEnViewData(
            ViewData, pagina, TamanoPagina, resultado.Total, nameof(Index),
            rotas: new Dictionary<string, string?> { ["busqueda"] = busqueda });

        return View(resultado);
    }

    //------------------------------------------------------------ALTA --------------------------------------------------------
    public IActionResult Crear()
    {
        return View(new Cliente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Cliente cliente)
    {
        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        cliente.Nombre = cliente.Nombre.Trim();

        int id = _repositorioCliente.CrearCliente(cliente);

        if (id <= 0)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el cliente.");
            return View(cliente);
        }

        TempData["Exito"] = $"Cliente '{cliente.Nombre}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    //------------------------------------------------------------EDICION --------------------------------------------------------
    public IActionResult Editar(int id)
    {
        var cliente = _repositorioCliente.ObtenerPorId(id);

        if (cliente is null)
        {
            return NoExiste();
        }

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Cliente cliente)
    {
        if (id != cliente.Id)
        {
            ModelState.AddModelError(string.Empty, "El cliente editado no coincide con el solicitado.");
            return View(cliente);
        }

        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        cliente.Nombre = cliente.Nombre.Trim();

        if (!_repositorioCliente.ActualizarCliente(cliente))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el cliente.");
            return View(cliente);
        }

        TempData["Exito"] = $"Cliente '{cliente.Nombre}' actualizado correctamente.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    //------------------------------------------------------------DETALLES CON HISTORIAL DE COMPRAS --------------------------------------------------------
    public IActionResult Detalles(int id, int pagina = 1)
    {
        var cliente = _repositorioCliente.ObtenerPorId(id);

        if (cliente is null)
        {
            return NoExiste();
        }

        var compras = _repositorioCliente.ObtenerCompras(id, pagina, TamanoPaginaHistorial);

        AyudantePaginacion.CargarEnViewData(
            ViewData, pagina, TamanoPaginaHistorial, compras.Total, nameof(Detalles),
            rotas: new Dictionary<string, string?> { ["id"] = id.ToString() });

        return View(new DetalleClienteViewModel
        {
            Cliente = cliente,
            Compras = compras
        });
    }

    //------------------------------------------------------------BAJA LOGICA / REACTIVAR --------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DarDeBaja(int id)
    {
        return CambiarEstado(id, false, "dado de baja");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Activar(int id)
    {
        return CambiarEstado(id, true, "reactivado");
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private IActionResult CambiarEstado(int id, bool nuevoEstado, string accion)
    {
        var cliente = _repositorioCliente.ObtenerPorId(id);

        if (cliente is null)
        {
            return NoExiste();
        }

        bool cambiado = _repositorioCliente.CambiarEstadoCliente(id, nuevoEstado);

        TempData[cambiado ? "Exito" : "Error"] = cambiado
            ? $"Cliente '{cliente.Nombre}' fue {accion}."
            : $"No se pudo cambiar el estado del cliente '{cliente.Nombre}'.";

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NoExiste()
    {
        TempData["Error"] = "El cliente solicitado no existe.";
        return RedirectToAction(nameof(Index));
    }
}
