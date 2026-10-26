using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>Una linea de la receta de un producto, para mostrarla en pantalla.</summary>
public class RecetaItem
{
    public int InsumoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public UnidadMedida Unidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public decimal Cantidad { get; set; }

    /// <summary>Costo que aporta esta linea a la receta (costo unitario x cantidad).</summary>
    public decimal Subtotal => CostoUnitario * Cantidad;
}
