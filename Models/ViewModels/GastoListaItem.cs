using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Fila resumen de un gasto con el nombre del usuario que lo registro.</summary>
public class GastoListaItem
{
    public int Id { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public CategoriaGasto Categoria { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }
}
