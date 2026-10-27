namespace Proyecto_GIPE.Models.ViewModels;

public class RentabilidadProducto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal CantidadVendida { get; set; }

    public decimal Ingresos { get; set; }

    public decimal Costo { get; set; }

    public decimal Ganancia { get; set; }

    public decimal Margen => Ingresos == 0 ? 0 : Ganancia / Ingresos * 100;
}
