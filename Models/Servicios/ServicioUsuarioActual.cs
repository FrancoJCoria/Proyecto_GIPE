using System.Globalization;
using System.Security.Claims;

namespace Proyecto_GIPE.Models.Servicios;

/// <summary>
/// Devuelve el id del usuario con sesion activa, tomado del claim NameIdentifier.
/// Si no hay sesion (no deberia pasar: los controllers estan protegidos con [Authorize]),
/// lanza excepcion para no registrar movimientos de un usuario inexistente.
/// </summary>
public class ServicioUsuarioActual : IServicioUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServicioUsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int ObtenerUsuarioId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(claim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
        {
            return id;
        }

        throw new InvalidOperationException(
            "No hay un usuario autenticado en la sesion actual para asociar el movimiento.");
    }
}
