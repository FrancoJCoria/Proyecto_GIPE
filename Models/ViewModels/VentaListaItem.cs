namespace Proyecto_GIPE.Models.ViewModels;

/// <summary>
/// Fila resumen de una venta, usada en listados (Ventas) y en el historial de compras de un cliente.
/// Las queries hacen JOIN con cliente y usuario para traer los nombres ya resueltos.
/// </summary>
public class VentaListaItem
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public string? ClienteNombre { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public decimal MontoTotal { get; set; }

    public bool Estado { get; set; }
}
