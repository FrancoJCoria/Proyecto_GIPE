using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers;

// La gestion de insumos (costos internos) queda reservada al Dueno.
[Authorize(Policy = "Dueno")]
public class InsumosController : Controller
{
    private const int TamanoPagina = 10;

    private readonly IRepositorioInsumo _repositorioInsumo;

    public InsumosController(IRepositorioInsumo repositorioInsumo)
    {
        _repositorioInsumo = repositorioInsumo;
    }

    //------------------------------------------------------------LISTADO PAGINADO --------------------------------------------------------
    public IActionResult Index(int pagina = 1, string? busqueda = null)
    {
        var resultado = _repositorioInsumo.ObtenerTodos(pagina, TamanoPagina, busqueda);

        ViewData["Busqueda"] = busqueda;
        AyudantePaginacion.CargarEnViewData(
            ViewData, pagina, TamanoPagina, resultado.Total, nameof(Index),
            rotas: new Dictionary<string, string?> { ["busqueda"] = busqueda });

        return View(resultado);
    }

    //------------------------------------------------------------ALTA --------------------------------------------------------
    public IActionResult Crear()
    {
        return View(new Insumo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Insumo insumo)
    {
        if (!ModelState.IsValid)
        {
            return View(insumo);
        }

        insumo.Nombre = insumo.Nombre.Trim();

        int id = _repositorioInsumo.CrearInsumo(insumo);

        if (id <= 0)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el insumo.");
            return View(insumo);
        }

        TempData["Exito"] = $"Insumo '{insumo.Nombre}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    //------------------------------------------------------------EDICION --------------------------------------------------------
    public IActionResult Editar(int id)
    {
        var insumo = _repositorioInsumo.ObtenerPorId(id);

        if (insumo is null)
        {
            return NoExiste();
        }

        return View(insumo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Insumo insumo)
    {
        if (id != insumo.Id)
        {
            ModelState.AddModelError(string.Empty, "El insumo editado no coincide con el solicitado.");
            return View(insumo);
        }

        if (!ModelState.IsValid)
        {
            return View(insumo);
        }

        insumo.Nombre = insumo.Nombre.Trim();

        if (!_repositorioInsumo.ActualizarInsumo(insumo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el insumo.");
            return View(insumo);
        }

        TempData["Exito"] = $"Insumo '{insumo.Nombre}' actualizado correctamente.";
        return RedirectToAction(nameof(Index));
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
        var insumo = _repositorioInsumo.ObtenerPorId(id);

        if (insumo is null)
        {
            return NoExiste();
        }

        bool cambiado = _repositorioInsumo.CambiarEstadoInsumo(id, nuevoEstado);

        TempData[cambiado ? "Exito" : "Error"] = cambiado
            ? $"Insumo '{insumo.Nombre}' fue {accion}."
            : $"No se pudo cambiar el estado del insumo '{insumo.Nombre}'.";

        return RedirectToAction(nameof(Index));
    }

    private IActionResult NoExiste()
    {
        TempData["Error"] = "El insumo solicitado no existe.";
        return RedirectToAction(nameof(Index));
    }
}
