using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioGasto
{
    /// <summary>Listado paginado con filtros opcionales de fecha y categoria.</summary>
    ResultadoPaginado<GastoListaItem> ObtenerTodos(
        int pagina, int tamano,
        DateTime? desde = null, DateTime? hasta = null, CategoriaGasto? categoria = null);

    Gasto? ObtenerPorId(int id);

    /// <summary>Inserta el gasto y devuelve el id generado (0 si fallo).</summary>
    int CrearGasto(Gasto gasto);

    bool ActualizarGasto(Gasto gasto);

    /// <summary>Elimina fisicamente el gasto (la tabla no tiene columna estado).</summary>
    bool EliminarGasto(int id);
}
