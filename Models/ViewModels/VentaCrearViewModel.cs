using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Modelo del formulario de alta de una venta. Los items se cargan dinamicamente con Select2.</summary>
public class VentaCrearViewModel
{
    /// <summary>Cliente asociado a la venta; opcional (venta mostrador).</summary>
    public int? ClienteId { get; set; }

    public IList<ItemVentaInput> Items { get; set; } = [new ItemVentaInput()];
}

/// <summary>Una fila del formulario: producto elegido con Select2 + cantidad.</summary>
public class ItemVentaInput
{
    /// <summary>Nulo si la fila quedo sin producto elegido (el controlador ignora esa fila).</summary>
    public int? ProductoId { get; set; }

    public decimal? Cantidad { get; set; } = 1;

    /// <summary>True si la fila tiene un producto elegido con una cantidad no valida.</summary>
    public bool ConProducto => ProductoId is > 0;
}
