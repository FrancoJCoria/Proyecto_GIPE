using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Proyecto_GIPE.Models.Entities;

namespace Proyecto_GIPE.Models.Servicios;

public class ServicioJwt
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _horasExpiracion;

    public ServicioJwt(IConfiguration configuration)
    {
        _secretKey = configuration["TokenAuthentication:SecretKey"]
            ?? throw new InvalidOperationException("Falta la clave secreta para generar el JWT.");
        _issuer = configuration["TokenAuthentication:Issuer"] ?? "GIPE";
        _audience = configuration["TokenAuthentication:Audience"] ?? "GIPE";
        _horasExpiracion = int.TryParse(
            configuration["TokenAuthentication:ExpirationHours"],
            out var horas)
            ? horas
            : 8;
    }

    public TokenJwt GenerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            SecurityAlgorithms.HmacSha256);

        var expiracion = DateTime.UtcNow.AddHours(_horasExpiracion);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiracion,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credenciales
        };

        var manejador = new JwtSecurityTokenHandler();
        var token = manejador.CreateToken(descriptor);

        return new TokenJwt(manejador.WriteToken(token), expiracion);
    }
}

public sealed record TokenJwt(string Token, DateTime Expira);
