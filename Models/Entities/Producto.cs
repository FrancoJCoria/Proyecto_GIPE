using System.ComponentModel.DataAnnotations;
using Microsoft.Net.Http.Headers;
using Proyecto_GIPE.Models;

public class Producto()
{
    [Key]
    public int Id {get; set;}


    [Required]
    public string Nombre {get; set;} = string.Empty;

    [Required]
    public decimal CostoExtra {get; set;} 

    [Required]
    public int MargenObjetivo{get; set;}

    [Required]
    public decimal PrecioActual{get; set;}

    [Required]
    public int StockActual {get; set;}

    
    public string UrlProducto {get; set;}

    public bool Estado {get; set;}
}