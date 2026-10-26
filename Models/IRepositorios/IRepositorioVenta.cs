using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioVenta
{
    /// <summary>
    /// Listado paginado de ventas, mas recientes primero. Si se indica usuarioId,
    /// solo devuelve las ventas registradas por ese usuario (empleado).
    /// </summary>
    ResultadoPaginado<VentaListaItem> ObtenerTodos(int pagina, int tamano, int? usuarioId = null);

    /// <summary>Venta con su cliente (si tiene), el usuario que la registro y sus items.</summary>
    VentaDetalleViewModel? ObtenerDetalle(int ventaId);

    /// <summary>
    /// Inserta la venta, sus items (congelando precio y costo al momento) y descuenta el stock
    /// de los productos. Todo dentro de una transaccion: si algo falla, no se guarda nada.
    /// Devuelve el id de la venta creada.
    /// </summary>
    /// <exception cref="Proyecto_GIPE.Models.Excepciones.ReglaDeNegocioException">
    /// Si el cliente, el usuario o algun producto no existe/esta inactivo.
    /// </exception>
    /// <exception cref="Proyecto_GIPE.Models.Excepciones.StockInsuficienteException">
    /// Si algun producto no tiene stock suficiente para la cantidad pedida.
    /// </exception>
    int CrearVenta(int? clienteId, int usuarioId, IList<LineaVenta> lineas);
}
