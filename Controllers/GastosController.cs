using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Controllers;

// Los gastos solo los gestiona el Dueno.
[Authorize(Policy = "Dueno")]
public class GastosController : Controller
{
    private const int TamanoPagina = 10;

    private readonly IRepositorioGasto _repositorioGasto;
    private readonly IServicioUsuarioActual _servicioUsuarioActual;

    public GastosController(
        IRepositorioGasto repositorioGasto,
        IServicioUsuarioActual servicioUsuarioActual)
    {
        _repositorioGasto = repositorioGasto;
        _servicioUsuarioActual = servicioUsuarioActual;
    }

    //------------------------------------------------------------LISTADO PAGINADO CON FILTROS --------------------------------------------------------
    public IActionResult Index(GastoFiltrosViewModel filtros)
    {
        filtros ??= new GastoFiltrosViewModel();

        if (filtros.Desde.HasValue && filtros.Hasta.HasValue && filtros.Desde > filtros.Hasta)
        {
            ModelState.AddModelError("Filtros.Desde", "La fecha 'desde' debe ser anterior o igual a la fecha 'hasta'.");
            filtros.Desde = null;
            filtros.Hasta = null;
        }

        var resultado = _repositorioGasto.ObtenerTodos(
            filtros.Pagina, TamanoPagina, filtros.Desde, filtros.Hasta, filtros.Categoria);

        var rotas = new Dictionary<string, string?>();

        if (filtros.Desde.HasValue)
        {
            rotas["desde"] = filtros.Desde.Value.ToString("yyyy-MM-dd");
        }

        if (filtros.Hasta.HasValue)
        {
            rotas["hasta"] = filtros.Hasta.Value.ToString("yyyy-MM-dd");
        }

        if (filtros.Categoria.HasValue)
        {
            rotas["categoria"] = filtros.Categoria.Value.ToString();
        }

        AyudantePaginacion.CargarEnViewData(
            ViewData, filtros.Pagina, TamanoPagina, resultado.Total, nameof(Index), rotas: rotas);

        return View(new GastosIndexViewModel
        {
            Filtros = filtros,
            Resultado = resultado
        });
    }

    //------------------------------------------------------------ALTA --------------------------------------------------------
    public IActionResult Crear()
    {
        return View(new Gasto { Fecha = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Gasto gasto)
    {
        if (!ModelState.IsValid)
        {
            return View(gasto);
        }

        // El id viene del claim del usuario con sesion activa ([Authorize(Policy = "Dueno")]).
        gasto.UsuarioId = _servicioUsuarioActual.ObtenerUsuarioId();
        gasto.Descripcion = gasto.Descripcion.Trim();

        int id = _repositorioGasto.CrearGasto(gasto);

        if (id <= 0)
        {
            ModelState.AddModelError(string.Empty, "No se pudo registrar el gasto.");
            return View(gasto);
        }

        TempData["Exito"] = $"Gasto '{gasto.Descripcion}' registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    //------------------------------------------------------------EDICION --------------------------------------------------------
    public IActionResult Editar(int id)
    {
        var gasto = _repositorioGasto.ObtenerPorId(id);

        if (gasto is null)
        {
            return NoExiste();
        }

        return View(gasto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, Gasto gasto)
    {
        if (id != gasto.Id)
        {
            ModelState.AddModelError(string.Empty, "El gasto editado no coincide con el solicitado.");
            return View(gasto);
        }

        if (!ModelState.IsValid)
        {
            return View(gasto);
        }

        // Se parte del gasto guardado para no perder el usuario que lo registro.
        var gastoGuardado = _repositorioGasto.ObtenerPorId(id);

        if (gastoGuardado is null)
        {
            return NoExiste();
        }

        gastoGuardado.Descripcion = gasto.Descripcion.Trim();
        gastoGuardado.Categoria = gasto.Categoria;
        gastoGuardado.Monto = gasto.Monto;
        gastoGuardado.Fecha = gasto.Fecha;

        if (!_repositorioGasto.ActualizarGasto(gastoGuardado))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el gasto.");
            return View(gastoGuardado);
        }

        TempData["Exito"] = $"Gasto '{gastoGuardado.Descripcion}' actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    //------------------------------------------------------------BAJA (FISICA, NO HAY ESTADO) --------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        var gasto = _repositorioGasto.ObtenerPorId(id);

        if (gasto is null)
        {
            return NoExiste();
        }

        bool eliminado = _repositorioGasto.EliminarGasto(id);

        TempData[eliminado ? "Exito" : "Error"] = eliminado
            ? $"Gasto '{gasto.Descripcion}' eliminado."
            : $"No se pudo eliminar el gasto '{gasto.Descripcion}'.";

        return RedirectToAction(nameof(Index));
    }

    //------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private IActionResult NoExiste()
    {
        TempData["Error"] = "El gasto solicitado no existe.";
        return RedirectToAction(nameof(Index));
    }
}
