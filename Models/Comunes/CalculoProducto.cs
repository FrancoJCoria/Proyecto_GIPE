namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Resultado del calculo de costos y precios de un producto, con los valores al momento.
/// No se persiste: sirve para la ficha del producto, la calculadora en vivo y el JSON de consulta.
/// </summary>
/// <param name="CostoInsumos">Suma de (cantidad x costo unitario) de las lineas de la receta.</param>
/// <param name="CostoExtra">Gastos que no dependen de insumos, definidos en el producto.</param>
/// <param name="CostoTotal">CostoInsumos + CostoExtra, redondeado a 2 decimales.</param>
/// <param name="MargenObjetivo">Margen porcentual con el que se calcula el precio sugerido (0 a 100).</param>
/// <param name="PrecioActual">Precio de venta vigente en el producto.</param>
/// <param name="PrecioSugerido">CostoTotal aplicando el margen objetivo.</param>
/// <param name="MargenActual">Margen real del PrecioActual sobre el CostoTotal (%).</param>
/// <param name="GananciaUnitaria">PrecioActual - CostoTotal.</param>
public sealed record CalculoProducto(
    decimal CostoInsumos,
    decimal CostoExtra,
    decimal CostoTotal,
    decimal MargenObjetivo,
    decimal PrecioActual,
    decimal PrecioSugerido,
    decimal MargenActual,
    decimal GananciaUnitaria);
