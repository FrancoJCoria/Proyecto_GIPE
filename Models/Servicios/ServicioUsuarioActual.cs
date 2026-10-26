using System.Globalization;
using System.Security.Claims;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Models.Servicios;

/// <summary>
/// Mientras no exista autenticacion, devuelve el id del usuario con sesion activa si hay claims,
/// y si no, hace fallback al primer usuario activo (el seed crea un Dueno y un Empleado).
/// TODO Fase 4 (autenticacion/roles): cuando este el login, leer el NameIdentifier del usuario
/// autenticado y lanzar excepcion si no hay sesion en vez de usar el fallback.
/// </summary>
public class ServicioUsuarioActual : IServicioUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepositorioUsuario _repositorioUsuario;

    public ServicioUsuarioActual(
        IHttpContextAccessor httpContextAccessor,
        IRepositorioUsuario repositorioUsuario)
    {
        _httpContextAccessor = httpContextAccessor;
        _repositorioUsuario = repositorioUsuario;
    }

    public int ObtenerUsuarioId()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(claim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
        {
            return id;
        }

        // TODO Fase 4: reemplazar este fallback por "no autenticado" cuando exista el login.
        var usuarios = _repositorioUsuario.ObtenerTodos();

        var usuario = usuarios.FirstOrDefault(u => u.Estado && u.Rol == RolUsuario.Dueno)
                      ?? usuarios.FirstOrDefault(u => u.Estado);

        return usuario?.Id ?? throw new InvalidOperationException(
            "No hay usuarios activos en la base para asociar el movimiento. Ejecuta data/script.sql y el seed.");
    }
}
