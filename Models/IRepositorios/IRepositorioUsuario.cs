using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.IRepositorios;

public interface IRepositorioUsuario
{
    Task<IEnumerable<Usuario>> ObtenerTodos();
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<int> CrearUsuario (Usuario usuario);
    Task<bool> ActualizarUsuario (Usuario usuario);
    Task<bool> CambiarEstadoUsuario (int id, bool nuevoEstado);
}