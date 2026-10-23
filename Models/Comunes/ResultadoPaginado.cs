namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Pagina de resultados que devuelven los repositorios en los listados paginados.
/// </summary>
public class ResultadoPaginado<T>
{
    /// <summary>Filas que corresponden a la pagina pedida.</summary>
    public IList<T> Filas { get; set; } = [];

    /// <summary>Cantidad total de registros que cumplen el filtro, sin aplicar paginacion.</summary>
    public int Total { get; set; }

    /// <summary>Indica si la pagina trajo al menos un registro.</summary>
    public bool HayFilas => Filas.Count > 0;
}
