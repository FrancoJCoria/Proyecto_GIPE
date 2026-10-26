namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Ayuda a armar el <see cref="PaginacionViewModel"/> que usan las vistas paginadas.
/// </summary>
public static class AyudantePaginacion
{
    public const string ClaveViewData = "Paginacion";

    /// <summary>
    /// Calcula la paginacion para una lista de resultados y devuelve el modelo
    /// listo para la vista parcial (queda guardado en ViewData con la clave "Paginacion").
    /// </summary>
    public static void CargarEnViewData(
        Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData,
        int pagina, int tamano, int total, string accion,
        string? controlador = null, Dictionary<string, string?>? rotas = null)
    {
        pagina = Math.Max(1, pagina);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        pagina = Math.Min(pagina, totalPaginas);

        viewData[AyudantePaginacion.ClaveViewData] = new PaginacionViewModel
        {
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Accion = accion,
            Controlador = controlador,
            Rotas = rotas ?? []
        };
    }
}
