using System.Data;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly IDbConnection _connection;

    public RepositorioUsuario(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodos(){}

    public async Task<Usuario?> ObtenerPorEmail(string email){}

    public async Task<int> CrearUsuario(Usuario usuario){}
}