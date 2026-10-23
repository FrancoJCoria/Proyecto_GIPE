namespace Proyecto_GIPE.Models.Excepciones;

/// <summary>
/// Se lanza cuando la cantidad pedida en una venta supera el stock disponible del producto.
/// </summary>
public class StockInsuficienteException : ReglaDeNegocioException
{
    public StockInsuficienteException(string producto, decimal stockDisponible, decimal cantidadPedida)
        : base($"Stock insuficiente de '{producto}': hay {stockDisponible:0.####} disponible y se pidieron {cantidadPedida:0.####}.")
    {
        Producto = producto;
        StockDisponible = stockDisponible;
        CantidadPedida = cantidadPedida;
    }

    public string Producto { get; }

    public decimal StockDisponible { get; }

    public decimal CantidadPedida { get; }
}
