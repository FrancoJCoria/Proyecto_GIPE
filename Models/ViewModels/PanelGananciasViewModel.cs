using Proyecto_GIPE.Models.Comunes;

namespace Proyecto_GIPE.Models.ViewModels;

public class PanelGananciasViewModel
{
    public DateTime Desde { get; set; }

    public DateTime Hasta { get; set; }

    public decimal Ingresos { get; set; }

    public decimal CostoVentas { get; set; }

    public decimal Gastos { get; set; }

    public decimal GananciaNeta => Ingresos - CostoVentas - Gastos;

    public decimal Margen => Ingresos == 0 ? 0 : GananciaNeta / Ingresos * 100;

    public ResultadoPaginado<RentabilidadProducto> Productos { get; set; } = new();
}
