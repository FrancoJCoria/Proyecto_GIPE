using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioGanancia
{
    decimal ObtenerIngresos(DateTime desde, DateTime hasta);

    decimal ObtenerCostoVentas(DateTime desde, DateTime hasta);

    decimal ObtenerGastos(DateTime desde, DateTime hasta);

    ResultadoPaginado<RentabilidadProducto> ObtenerRentabilidadPorProducto(
        DateTime desde, DateTime hasta, int pagina, int tamano);
}
