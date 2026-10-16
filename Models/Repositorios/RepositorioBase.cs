using Microsoft.Extensions.Configuration;

namespace Proyecto_GIPE.Models.Repositorios;

public abstract class RepositorioBase
{
    protected readonly string connectionString;

    public RepositorioBase(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
}