using Microsoft.AspNetCore.Identity;
using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.Servicios;

public class ServicioPassword
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string HashearPassword(Usuario usuario, string password)
    {
        return _hasher.HashPassword(usuario, password);
    }

    public bool VerificarPassword(Usuario usuario, string password)
    {
        var resultado = _hasher.VerifyHashedPassword(
        usuario,
        usuario.PasswordHash,
        password);

        return resultado != PasswordVerificationResult.Failed;
    }
}