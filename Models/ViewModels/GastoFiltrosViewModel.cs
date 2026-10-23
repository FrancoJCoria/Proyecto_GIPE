using System.ComponentModel.DataAnnotations;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Filtros y pagina actual del listado de gastos.</summary>
public class GastoFiltrosViewModel
{
    /// <summary>Fecha desde (incluida). Nulo = sin limite inferior.</summary>
    public DateTime? Desde { get; set; }

    /// <summary>Fecha hasta (incluida). Nulo = sin limite superior.</summary>
    public DateTime? Hasta { get; set; }

    /// <summary>Categoria a filtrar; nulo = todas.</summary>
    public CategoriaGasto? Categoria { get; set; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;
}
