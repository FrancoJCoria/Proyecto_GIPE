using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_GIPE.Models.DTOs.Usuarios;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Servicios;
using Proyecto_GIPE.ViewModels;

namespace Proyecto_GIPE.Controllers;

public class AccountController : Controller
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ServicioImagenes _servicioImagenes;

    public AccountController(
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher,
        ServicioImagenes servicioImagenes)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _servicioImagenes = servicioImagenes;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = _repositorioUsuario.ObtenerPorEmail(model.Email.Trim());

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        if (!usuario.Estado)
        {
            ModelState.AddModelError(string.Empty, "El usuario está deshabilitado. Contactá al dueño.");
            return View(model);
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            model.Password);

        if (resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        // El avatar viaja en la cookie para poder mostrarlo en el layout sin golpear la BD.
        if (!string.IsNullOrWhiteSpace(usuario.Url_avatar))
        {
            claims.Add(new Claim("GIPE.Avatar", usuario.Url_avatar));
        }

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var propiedades = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        };

        if (model.RememberMe)
        {
            propiedades.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            propiedades);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var id = ObtenerUsuarioId();

        if (id is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id.Value);

        if (usuario is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        return View(new UsuarioRespuestaDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            Url_avatar = usuario.Url_avatar
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    //------------------------------------------------------------AVATAR --------------------------------------------------------
    // Subir/reemplazar la foto de perfil del usuario logueado. El archivo subido
    // tiene prioridad sobre la url externa; ambas se aceptan (perfil y ABM de usuarios).

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarAvatar(IFormFile? archivoAvatar)
    {
        var id = ObtenerUsuarioId();

        if (id is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id.Value);

        if (usuario is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (archivoAvatar is null || archivoAvatar.Length == 0)
        {
            TempData["Error"] = "Seleccioná una imagen para subir.";
            return RedirectToAction(nameof(Perfil));
        }

        string? urlNueva;

        try
        {
            urlNueva = _servicioImagenes.Guardar(archivoAvatar, "Avatares", usuario.Url_avatar);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Perfil));
        }

        if (urlNueva is null || _repositorioUsuario.EditarAvatarUsuario(usuario.Id, urlNueva) is null)
        {
            TempData["Error"] = "No se pudo guardar la foto de perfil.";
            return RedirectToAction(nameof(Perfil));
        }

        // Re-firmo la sesion para que el claim del avatar quede actualizado en la cookie.
        usuario.Url_avatar = urlNueva;
        await IniciarSesion(usuario);

        TempData["Mensaje"] = "Foto de perfil actualizada.";
        return RedirectToAction(nameof(Perfil));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarAvatar()
    {
        var id = ObtenerUsuarioId();

        if (id is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id.Value);

        if (usuario is null)
        {
            return RedirectToAction(nameof(Login));
        }

        _servicioImagenes.BorrarSiLocal("Avatares", usuario.Url_avatar);

        if (_repositorioUsuario.EditarAvatarUsuario(usuario.Id, null!) is null)
        {
            TempData["Error"] = "No se pudo quitar la foto de perfil.";
            return RedirectToAction(nameof(Perfil));
        }

        usuario.Url_avatar = null;
        await IniciarSesion(usuario);

        TempData["Mensaje"] = "Foto de perfil eliminada.";
        return RedirectToAction(nameof(Perfil));
    }

    private async Task IniciarSesion(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuario.Url_avatar))
        {
            claims.Add(new Claim("GIPE.Avatar", usuario.Url_avatar));
        }

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : null;
    }
}
