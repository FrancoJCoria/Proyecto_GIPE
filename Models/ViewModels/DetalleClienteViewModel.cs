using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Modelo de la vista Detalles de un cliente: ficha + historial de compras paginado.</summary>
public class DetalleClienteViewModel
{
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Ventas asociadas al cliente, mas recientes primero.</summary>
    public ResultadoPaginado<VentaListaItem> Compras { get; set; } = new();
}
