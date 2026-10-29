using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioProducto
{
    /// <summary>Listado paginado con busqueda opcional por nombre. Con soloActivos filtra la columna estado.</summary>
    ResultadoPaginado<Producto> ObtenerTodos(int pagina, int tamano, string? busqueda = null, bool soloActivos = false);

    /// <summary>Productos activos, ordenados por nombre (para el desplegable de ventas).</summary>
    IList<Producto> ObtenerActivos();

    Producto? ObtenerPorId(int id);

    /// <summary>Inserta el producto y devuelve el id generado (0 si fallo).</summary>
    int CrearProducto(Producto producto);

    /// <summary>Actualiza datos del producto, incluido el stock (el stock se mantiene manual).</summary>
    bool ActualizarProducto(Producto producto);

    /// <summary>Da de baja o reactiva logicamente un producto (columna estado).</summary>
    bool CambiarEstadoProducto(int id, bool nuevoEstado);

    /// <summary>Reemplaza el stock actual por el valor indicado (carga manual de stock).</summary>
    bool ActualizarStock(int id, decimal stock);

    /// <summary>Costo de produccion: suma de insumos de la receta + costo extra; nulo si no existe.</summary>
    decimal? ObtenerCostoProducto(int id);

    /// <summary>Receta del producto con los datos del insumo para mostrarla.</summary>
    IList<RecetaItem> ObtenerReceta(int productoId);

    /// <summary>Reemplaza la receta completa del producto por la lista indicada.</summary>
    bool GuardarReceta(int productoId, IList<ProductoInsumo> lineas);

    /// <summary>Quita una linea de la receta. Devuelve false si la linea no existia.</summary>
    bool EliminarLineaReceta(int productoId, int insumoId);
}
