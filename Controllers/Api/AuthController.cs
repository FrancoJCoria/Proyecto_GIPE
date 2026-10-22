using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Usuarios;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ServicioJwt _servicioJwt;

    public AuthController(
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher,
        ServicioJwt servicioJwt)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _servicioJwt = servicioJwt;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginUsuarioDto dto)
    {
        var usuario = _repositorioUsuario.ObtenerPorEmail(dto.Email.Trim());

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        if (!usuario.Estado)
        {
            return Unauthorized(new { mensaje = "El usuario está deshabilitado." });
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            dto.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        var token = _servicioJwt.GenerarToken(usuario);

        return Ok(new
        {
            token = token.Token,
            tipo = "Bearer",
            expira = token.Expira,
            usuario = new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Estado = usuario.Estado,
                Url_avatar = usuario.Url_avatar
            }
        });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idClaim, out var id))
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id);

        if (usuario is null)
        {
            return NotFound(new { mensaje = "El usuario no existe." });
        }

        return Ok(new UsuarioRespuestaDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            Url_avatar = usuario.Url_avatar
        });
    }
}
