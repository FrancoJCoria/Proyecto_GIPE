using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioInsumo
{
    /// <summary>Listado paginado con busqueda opcional por nombre. Con soloActivos filtra la columna estado.</summary>
    ResultadoPaginado<Insumo> ObtenerTodos(int pagina, int tamano, string? busqueda = null, bool soloActivos = false);

    /// <summary>Insumos activos, ordenados por nombre (para las recetas de productos).</summary>
    IList<Insumo> ObtenerActivos();

    Insumo? ObtenerPorId(int id);

    /// <summary>Inserta el insumo y devuelve el id generado (0 si fallo).</summary>
    int CrearInsumo(Insumo insumo);

    bool ActualizarInsumo(Insumo insumo);

    /// <summary>Da de baja o reactiva logicamente un insumo (columna estado).</summary>
    bool CambiarEstadoInsumo(int id, bool nuevoEstado);
}
