using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Usuarios;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.ViewModels;

namespace Proyecto_GIPE.Controllers;

[Authorize(Policy = "Dueno")]
public class UsuariosController : Controller
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuariosController(
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher,
        ServicioImagenes servicioImagenes)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _servicioImagenes = servicioImagenes;
    }

    private readonly ServicioImagenes _servicioImagenes;

    [HttpGet]
    public IActionResult Index()
    {
        var usuarios = _repositorioUsuario.ObtenerTodos()
            .OrderBy(usuario => usuario.Id)
            .Select(usuario => new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Estado = usuario.Estado,
                Url_avatar = usuario.Url_avatar
            })
            .ToList();

        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new UsuarioFormViewModel
        {
            Rol = RolUsuario.Empleado,
            Estado = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(UsuarioFormViewModel model, IFormFile? archivoAvatar)
    {
        ValidarPasswordObligatoria(model);

        if (!Enum.IsDefined(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "Seleccioná un rol válido.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();

        if (_repositorioUsuario.ObtenerPorEmail(email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese correo.");
            return View(model);
        }

        var usuario = new Usuario
        {
            Nombre = model.Nombre.Trim(),
            Email = email,
            Rol = model.Rol,
            Estado = model.Estado,
            Url_avatar = string.IsNullOrWhiteSpace(model.Url_avatar)
                ? null
                : model.Url_avatar.Trim()
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password!);

        var idCreado = _repositorioUsuario.CrearUsuario(usuario);

        if (idCreado <= 0)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear el usuario.");
            return View(model);
        }

        TempData["Mensaje"] = "Usuario creado correctamente.";
        GuardarAvatarSiCorresponde(idCreado, archivoAvatar, null);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var usuario = _repositorioUsuario.ObtenerPorId(id);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(new UsuarioFormViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Url_avatar = usuario.Url_avatar,
            Estado = usuario.Estado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, UsuarioFormViewModel model, IFormFile? archivoAvatar)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id);

        if (usuario is null)
        {
            return NotFound();
        }

        ValidarPasswordOpcional(model);

        if (!Enum.IsDefined(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "Seleccioná un rol válido.");
        }

        var usuarioActualId = ObtenerUsuarioId();

        if (usuarioActualId.HasValue && usuarioActualId.Value == id)
        {
            if (model.Rol != RolUsuario.Dueno)
            {
                ModelState.AddModelError(
                    nameof(model.Rol),
                    "No podés quitarte el rol de dueño.");
            }

            if (!model.Estado)
            {
                ModelState.AddModelError(
                    nameof(model.Estado),
                    "No podés desactivar tu propio usuario.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var usuarioConEmail = _repositorioUsuario.ObtenerPorEmail(email);

        if (usuarioConEmail is not null && usuarioConEmail.Id != id)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Ese correo ya está registrado por otro usuario.");
            return View(model);
        }

        var avatarAnterior = usuario.Url_avatar;
        usuario.Nombre = model.Nombre.Trim();
        usuario.Email = email;
        usuario.Rol = model.Rol;
        usuario.Estado = model.Estado;
        usuario.Url_avatar = string.IsNullOrWhiteSpace(model.Url_avatar)
            ? null
            : model.Url_avatar.Trim();

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, model.Password);
        }

        if (!_repositorioUsuario.ActualizarUsuario(usuario))
        {
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario.");
            return View(model);
        }

        // Si subieron archivo, gana sobre la url; "Guardar" ya borro la imagen local anterior.
        if (archivoAvatar is not null && archivoAvatar.Length > 0)
        {
            GuardarAvatarSiCorresponde(id, archivoAvatar, avatarAnterior);
        }
        else if (string.IsNullOrWhiteSpace(model.Url_avatar) &&
                 !string.IsNullOrWhiteSpace(avatarAnterior))
        {
            // Se limpio la referencia del avatar (o venia de un archivo local): si era
            // local se borra fisicamente para no dejar archivos huerfanos.
            _servicioImagenes.BorrarSiLocal("Avatares", avatarAnterior);
        }

        TempData["Mensaje"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id)
    {
        var usuario = _repositorioUsuario.ObtenerPorId(id);

        if (usuario is null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        var usuarioActualId = ObtenerUsuarioId();

        if (usuarioActualId.HasValue && usuarioActualId.Value == id)
        {
            TempData["Error"] = "No podés cambiar el estado de tu propio usuario.";
            return RedirectToAction(nameof(Index));
        }

        if (!_repositorioUsuario.CambiarEstadoUsuario(id, !usuario.Estado))
        {
            TempData["Error"] = "No se pudo cambiar el estado del usuario.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = usuario.Estado
            ? "Usuario desactivado correctamente."
            : "Usuario activado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sube la foto de avatar si vino archivo. Guarda el archivo fisico con
    /// ServicioImagenes (borrando el anterior si era local) y persiste la url
    /// con EditarAvatarUsuario. En error informa con TempData sin cortar el flujo.
    /// </summary>
    private void GuardarAvatarSiCorresponde(int id, IFormFile? archivo, string? urlAnterior)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return;
        }

        try
        {
            var urlNueva = _servicioImagenes.Guardar(archivo, "Avatares", urlAnterior);

            if (urlNueva is not null && _repositorioUsuario.EditarAvatarUsuario(id, urlNueva) is not null)
            {
                TempData["Mensaje"] = "Foto de perfil guardada.";
            }
            else
            {
                TempData["Error"] = "No se pudo asociar la foto al usuario.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
    }

    private void ValidarPasswordObligatoria(UsuarioFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria.");
            return;
        }

        ValidarLongitudPassword(model);
    }

    private void ValidarPasswordOpcional(UsuarioFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return;
        }

        ValidarLongitudPassword(model);
    }

    private void ValidarLongitudPassword(UsuarioFormViewModel model)
    {
        if (model.Password!.Length < 8)
        {
            ModelState.AddModelError(
                nameof(model.Password),
                "La contraseña debe tener al menos 8 caracteres.");
        }

        if (model.Password.Length > 100)
        {
            ModelState.AddModelError(
                nameof(model.Password),
                "La contraseña no puede superar los 100 caracteres.");
        }
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : null;
    }
}
