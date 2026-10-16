
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Usuarios;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.Models.Enums;

namespace Proyecto_GIPE.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly ServicioPassword _servicioPassword;

    public UsuarioController(
        IRepositorioUsuario repositorioUsuario,
        ServicioPassword servicioPassword)
    {
        _repositorioUsuario = repositorioUsuario;
        _servicioPassword = servicioPassword;
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(
        int id,
        [FromBody] ActualizarUsuarioDto dto)
    {
        //buscar al usuaario por id 
        var usuarioExiste = _repositorioUsuario.ObtenerPorId(id);

        if (usuarioExiste == null)
        {
            return NotFound(new {mensaje = "El usuario no existe"});
        }

        // comprobar que el email no sea de otro usuario
        var usuarioConEmail =
            _repositorioUsuario.ObtenerPorEmail(dto.Email.Trim());

        if (usuarioConEmail != null && usuarioConEmail.Id != id)
        {
            return Conflict(new {mensaje = "Ese email ya esta registrado por otro usuario"});
        }

        //modificar campos
        usuarioExiste.Nombre = dto.Nombre.Trim();
        usuarioExiste.Email = dto.Email.Trim();
        usuarioExiste.Url_avatar = dto.Url_avatar;
        usuarioExiste.Estado = dto.Estado;

        // guardar los cambios
        bool actualizado = _repositorioUsuario.ActualizarUsuario(usuarioExiste);

        if (!actualizado)
        {
            return StatusCode(500, new {mensaje = "No se pudo actualizar el usuario"});
        }

        //regresar la respuesta
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
        // Comprobar si el email ya existe
        var usuarioExistente = _repositorioUsuario.ObtenerPorEmail(dto.Email.Trim());

        if (usuarioExistente != null)
        {
            return Conflict(new {mensaje = "Ya existe un usuario con ese correo"});
        }

        // Crear el usuario con rol de empleado
        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = dto.Email.Trim(),
            Rol = RolUsuario.Empleado,
            Estado = true,
            Url_avatar = dto.Url_avatar
        };

        // Hashear la contraseña
        usuario.PasswordHash = _servicioPassword.HashearPassword(usuario, dto.Password);

        // Guardar el usuario
        int idCreado = _repositorioUsuario.CrearUsuario(usuario);

        if (idCreado <= 0)
        {
            return StatusCode(500, new{mensaje = "No se pudo crear el usuario"});
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

}
