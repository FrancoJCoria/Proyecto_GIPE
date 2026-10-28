using System.ComponentModel.DataAnnotations;

namespace Proyecto_GIPE.Models.DTOs.Ventas;

public sealed class CrearVentaApiDto
{
    public int? ClienteId { get; set; }

    [Required, MinLength(1)]
    public IList<ItemVentaApiDto> Items { get; set; } = [];
}

public sealed class ItemVentaApiDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999")]
    public decimal Cantidad { get; set; }
}
