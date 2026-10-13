using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioUsuario
{
    IList<Usuario> ObtenerTodos();
    Usuario? ObtenerPorEmail(string email);
    int CrearUsuario(Usuario usuario);
    bool ActualizarUsuario(Usuario usuario);
    bool CambiarEstadoUsuario(int id, bool nuevoEstado);
}