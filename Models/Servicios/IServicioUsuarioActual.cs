namespace Proyecto_GIPE.Models.Servicios;

/// <summary>
/// Resuelve el usuario que registra movimientos (ventas y gastos), que en la base es un FK obligatorio.
/// </summary>
public interface IServicioUsuarioActual
{
    /// <summary>Id del usuario asociado a la peticion actual.</summary>
    int ObtenerUsuarioId();
}
