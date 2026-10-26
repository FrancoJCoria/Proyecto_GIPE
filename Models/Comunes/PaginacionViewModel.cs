namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Modelo que consume la vista parcial Shared/_Paginacion.cshtml.
/// </summary>
public class PaginacionViewModel
{
    public int Pagina { get; set; } = 1;

    public int TotalPaginas { get; set; } = 1;

    /// <summary>Accion de destino de los enlaces de paginacion.</summary>
    public string Accion { get; set; } = "Index";

    /// <summary>Controlador de destino; si queda vacio se usa el controlador de la vista actual.</summary>
    public string? Controlador { get; set; }

    /// <summary>Route values extra (busqueda, filtros, id) que se conservan al cambiar de pagina.</summary>
    public Dictionary<string, string?> Rotas { get; set; } = [];
}
