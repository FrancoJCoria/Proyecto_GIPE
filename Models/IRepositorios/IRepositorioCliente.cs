using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioCliente
{
    /// <summary>Listado paginado con busqueda opcional por nombre, apellido, email o telefono.</summary>
    ResultadoPaginado<Cliente> ObtenerTodos(int pagina, int tamano, string? busqueda = null);

    /// <summary>Clientes activos, ordenados por nombre (para el desplegable de ventas).</summary>
    IList<Cliente> ObtenerActivos();

    Cliente? ObtenerPorId(int id);

    /// <summary>Inserta el cliente y devuelve el id generado (0 si fallo).</summary>
    int CrearCliente(Cliente cliente);

    bool ActualizarCliente(Cliente cliente);

    /// <summary>Da de baja o reactiva logicamente un cliente (columna estado).</summary>
    bool CambiarEstadoCliente(int id, bool nuevoEstado);

    /// <summary>Historial de compras del cliente, paginado, mas recientes primero.</summary>
    ResultadoPaginado<VentaListaItem> ObtenerCompras(int clienteId, int pagina, int tamano);
}
