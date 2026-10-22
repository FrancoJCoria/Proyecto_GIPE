using Microsoft.AspNetCore.Identity;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Models.Servicios;

public class ServicioSeed
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ILogger<ServicioSeed> _logger;

    public ServicioSeed(
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher,
        ILogger<ServicioSeed> logger)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public void Ejecutar()
    {
        CrearSiNoExiste("Dueño GIPE", "dueno@gipe.com", "Admin123!", RolUsuario.Dueno);
        CrearSiNoExiste("Empleado GIPE", "empleado@gipe.com", "Empleado123!", RolUsuario.Empleado);
    }

    private void CrearSiNoExiste(string nombre, string email, string password, RolUsuario rol)
    {
        if (_repositorioUsuario.ObtenerPorEmail(email) is not null)
        {
            return;
        }

        var usuario = new Usuario
        {
            Nombre = nombre,
            Email = email,
            Rol = rol,
            Estado = true
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, password);

        var id = _repositorioUsuario.CrearUsuario(usuario);

        if (id <= 0)
        {
            throw new InvalidOperationException(
                $"No se pudo crear el usuario seed {email}. Verificá que data/script.sql se haya ejecutado.");
        }

        _logger.LogInformation("Usuario seed creado: {Email} ({Rol})", email, rol);
    }
}
