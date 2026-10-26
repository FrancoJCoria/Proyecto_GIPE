using Proyecto_GIPE.Models.Comunes;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.ViewModels;

namespace Proyecto_GIPE.Models.Servicios;

/// <summary>
/// Formulas centrales de costos y precios:
/// costo total = receta + costo extra; precio sugerido = costo total x (1 + margen/100);
/// margen real = (precio - costo total) / costo total x 100.
/// Se usan siempre los mismos redondeos (2 decimales) para que pantalla, JSON y guardado coincidan.
/// </summary>
public class ServicioCalculadora
{
    /// <summary>Redondeo comercial a 2 decimales usado en todos los montos.</summary>
    public static decimal Redondear2(decimal valor)
    {
        return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Suma de los subtotales de las lineas de la receta.</summary>
    public decimal CalcularCostoInsumos(IEnumerable<RecetaItem> receta)
    {
        return Redondear2(receta.Sum(r => r.Subtotal));
    }

    /// <summary>Costo de produccion: receta + costo extra del producto.</summary>
    public decimal CalcularCostoTotal(IEnumerable<RecetaItem> receta, decimal costoExtra)
    {
        return Redondear2(CalcularCostoInsumos(receta) + costoExtra);
    }

    /// <summary>Precio sugerido: costo total aplicando el margen objetivo (%), recortado entre 0 y 100.</summary>
    public decimal CalcularPrecioSugerido(decimal costoTotal, decimal margenObjetivo)
    {
        var margen = Math.Clamp(margenObjetivo, 0, 100);
        return Redondear2(costoTotal * (1 + margen / 100m));
    }

    /// <summary>Margen real del precio vigente sobre el costo total; 0 si el costo es cero.</summary>
    public decimal CalcularMargenReal(decimal costoTotal, decimal precioActual)
    {
        if (costoTotal <= 0)
        {
            return 0;
        }

        return Redondear2((precioActual - costoTotal) / costoTotal * 100m);
    }

    /// <summary>Ganancia por unidad vendida al precio vigente.</summary>
    public decimal CalcularGananciaUnitaria(decimal costoTotal, decimal precioActual)
    {
        return Redondear2(precioActual - costoTotal);
    }

    /// <summary>Calculo completo de un producto, opcionalmente con un margen distinto al guardado.</summary>
    public CalculoProducto Calcular(IEnumerable<RecetaItem> receta, Producto producto, decimal? margenObjetivo = null)
    {
        var costoInsumos = CalcularCostoInsumos(receta);
        var costoTotal = Redondear2(costoInsumos + producto.CostoExtra);
        var margen = Math.Clamp(margenObjetivo ?? producto.MargenObjetivo, 0, 100);

        return new CalculoProducto(
            costoInsumos,
            producto.CostoExtra,
            costoTotal,
            margen,
            producto.PrecioActual,
            CalcularPrecioSugerido(costoTotal, margen),
            CalcularMargenReal(costoTotal, producto.PrecioActual),
            CalcularGananciaUnitaria(costoTotal, producto.PrecioActual));
    }
}
