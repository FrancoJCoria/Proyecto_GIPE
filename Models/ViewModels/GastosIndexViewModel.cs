using Proyecto_GIPE.Models.Comunes;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Modelo de la vista principal de Gastos: filtros + listado paginado.</summary>
public class GastosIndexViewModel
{
    public GastoFiltrosViewModel Filtros { get; set; } = new();

    public ResultadoPaginado<GastoListaItem> Resultado { get; set; } = new();
}
