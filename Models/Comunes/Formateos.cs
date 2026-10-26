using System.Globalization;

namespace Proyecto_GIPE.Models.Comunes;

/// <summary>
/// Ayudantes para dar formato consistente a montos y fechas en las vistas.
/// Se usa la cultura es-AR para que los numeros se muestren con coma decimal.
/// </summary>
public static class Formateos
{
    private static readonly CultureInfo culturaEsAr = CultureInfo.GetCultureInfo("es-AR");

    /// <summary>Formatea un monto como "$ 1.234,56".</summary>
    public static string Dinero(decimal monto)
    {
        return "$ " + monto.ToString("N2", culturaEsAr);
    }

    /// <summary>Formatea un numero con dos decimales (1.234,56).</summary>
    public static string Numero2(decimal valor)
    {
        return valor.ToString("N2", culturaEsAr);
    }

    /// <summary>Formatea una cantidad con hasta cuatro decimales, sin ceros de mas (2,5 kg).</summary>
    public static string Cantidad(decimal valor)
    {
        return valor.ToString("0.####", culturaEsAr);
    }

    /// <summary>Formatea una fecha como dd/MM/yyyy.</summary>
    public static string Fecha(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy", culturaEsAr);
    }

    /// <summary>Formatea una fecha y hora como dd/MM/yyyy HH:mm.</summary>
    public static string FechaHora(DateTime fecha)
    {
        return fecha.ToString("dd/MM/yyyy HH:mm", culturaEsAr);
    }
}
