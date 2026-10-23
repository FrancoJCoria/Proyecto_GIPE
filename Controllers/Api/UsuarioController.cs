using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Usuarios;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = "Dueno")]
public class UsuarioController : ControllerBase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuarioController(
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(
        int id,
        [FromBody] ActualizarUsuarioDto dto)
    {
        if (dto.Rol is null || !Enum.IsDefined(dto.Rol.Value))
        {
            return BadRequest(new { mensaje = "El rol es obligatorio y debe ser válido." });
        }

        var usuarioExiste = _repositorioUsuario.ObtenerPorId(id);

        if (usuarioExiste == null)
        {
            return NotFound(new { mensaje = "El usuario no existe" });
        }

        var usuarioActualId = ObtenerUsuarioId();

        if (usuarioActualId.HasValue && usuarioActualId.Value == id)
        {
            if (dto.Rol.Value != RolUsuario.Dueno)
            {
                return BadRequest(new { mensaje = "No podés quitarte el rol de dueño." });
            }

            if (!dto.Estado)
            {
                return BadRequest(new { mensaje = "No podés desactivar tu propio usuario." });
            }
        }

        var usuarioConEmail = _repositorioUsuario.ObtenerPorEmail(dto.Email.Trim());

        if (usuarioConEmail != null && usuarioConEmail.Id != id)
        {
            return Conflict(new { mensaje = "Ese email ya está registrado por otro usuario" });
        }

        usuarioExiste.Nombre = dto.Nombre.Trim();
        usuarioExiste.Email = dto.Email.Trim();
        usuarioExiste.Rol = dto.Rol.Value;
        usuarioExiste.Url_avatar = dto.Url_avatar;
        usuarioExiste.Estado = dto.Estado;

        bool actualizado = _repositorioUsuario.ActualizarUsuario(usuarioExiste);

        if (!actualizado)
        {
            return StatusCode(500, new { mensaje = "No se pudo actualizar el usuario" });
        }

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente",
            id = usuarioExiste.Id,
            nombre = usuarioExiste.Nombre,
            email = usuarioExiste.Email,
            rol = usuarioExiste.Rol,
            estado = usuarioExiste.Estado,
            url_avatar = usuarioExiste.Url_avatar
        });
    }

    [HttpPost]
    public IActionResult Crear([FromBody] CrearUsuarioDto dto)
    {
        if (!Enum.IsDefined(dto.Rol))
        {
            return BadRequest(new { mensaje = "El rol no es válido." });
        }

        var usuarioExistente = _repositorioUsuario.ObtenerPorEmail(dto.Email.Trim());

        if (usuarioExistente != null)
        {
            return Conflict(new { mensaje = "Ya existe un usuario con ese correo" });
        }

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = dto.Email.Trim(),
            Rol = dto.Rol,
            Estado = true,
            Url_avatar = dto.Url_avatar
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

        int idCreado = _repositorioUsuario.CrearUsuario(usuario);

        if (idCreado <= 0)
        {
            return StatusCode(500, new { mensaje = "No se pudo crear el usuario" });
        }

        return StatusCode(201, new
        {
            id = idCreado,
            nombre = usuario.Nombre,
            email = usuario.Email,
            rol = usuario.Rol,
            estado = usuario.Estado,
            url_avatar = usuario.Url_avatar,
            mensaje = "Usuario creado correctamente"
        });
    }

    [HttpGet]
    public IActionResult ObtenerTodos()
    {
        var usuarios = _repositorioUsuario.ObtenerTodos();

        var resultado = usuarios.Select(usuario => new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            email = usuario.Email,
            rol = usuario.Rol,
            estado = usuario.Estado,
            url_avatar = usuario.Url_avatar
        });

        return Ok(resultado);
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : null;
    }
}
