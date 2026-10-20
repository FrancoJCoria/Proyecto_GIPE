using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioUsuario
{
    IList<Usuario> ObtenerTodos();
    Usuario? ObtenerPorEmail(string email);

    Usuario? ObtenerPorId(int id);
    int CrearUsuario(Usuario usuario);
    bool ActualizarUsuario(Usuario usuario);
    bool CambiarEstadoUsuario(int id, bool nuevoEstado);
    string? EditarAvatarUsuario(int id, string url);
    
}