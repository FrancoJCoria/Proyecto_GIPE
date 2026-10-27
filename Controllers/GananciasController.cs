using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Controllers;

[Authorize(Policy = "Dueno")]
public class GananciasController : Controller
{
    private const int TamanoPagina = 10;

    private readonly IRepositorioGanancia _repositorioGanancia;

    public GananciasController(IRepositorioGanancia repositorioGanancia)
    {
        _repositorioGanancia = repositorioGanancia;
    }

    public IActionResult Index(DateTime? desde, DateTime? hasta, int pagina = 1)
    {
        var primerDiaDelMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var fechaDesde = desde ?? primerDiaDelMes;
        var fechaHasta = hasta ?? DateTime.Today;

        if (fechaDesde > fechaHasta)
        {
            ModelState.AddModelError(string.Empty, "La fecha 'desde' debe ser anterior o igual a la fecha 'hasta'. Se muestra el mes actual.");
            fechaDesde = primerDiaDelMes;
            fechaHasta = DateTime.Today;
        }

        var modelo = new PanelGananciasViewModel
        {
            Desde = fechaDesde,
            Hasta = fechaHasta,
            Ingresos = _repositorioGanancia.ObtenerIngresos(fechaDesde, fechaHasta),
            CostoVentas = _repositorioGanancia.ObtenerCostoVentas(fechaDesde, fechaHasta),
            Gastos = _repositorioGanancia.ObtenerGastos(fechaDesde, fechaHasta),
            Productos = _repositorioGanancia.ObtenerRentabilidadPorProducto(fechaDesde, fechaHasta, pagina, TamanoPagina)
        };

        var rotas = new Dictionary<string, string?>
        {
            ["desde"] = fechaDesde.ToString("yyyy-MM-dd"),
            ["hasta"] = fechaHasta.ToString("yyyy-MM-dd")
        };

        AyudantePaginacion.CargarEnViewData(
            ViewData, pagina, TamanoPagina, modelo.Productos.Total, nameof(Index), "Ganancias", rotas);

        return View(modelo);
    }
}
