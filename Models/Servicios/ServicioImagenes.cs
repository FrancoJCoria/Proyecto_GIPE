using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Proyecto_GIPE.Models.Servicios;

/// <summary>
/// Guardado y limpieza de imagenes subidas por los usuarios (avatares y portadas
/// de productos). Inspirado en el manejo de Uploads del proyecto inmobiliariaULP:
/// se valida extension y tamano ANTES de tocar disco, se usa un nombre Guid para
/// evitar colisiones y cache del navegador, y al reemplazar una foto local se
/// borra la anterior. Las urls se guardan relativas (/Uploads/...).
/// </summary>
public class ServicioImagenes
{
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TamanoMaximo = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _entorno;
    private readonly ILogger<ServicioImagenes> _logger;

    public ServicioImagenes(IWebHostEnvironment entorno, ILogger<ServicioImagenes> logger)
    {
        _entorno = entorno;
        _logger = logger;
    }

    /// <summary>
    /// Guarda el archivo subido en wwwroot/Uploads/{carpeta} y devuelve la url relativa.
    /// Devuelve null si no vino archivo. Lanza InvalidOperationException con un mensaje
    /// para mostrar al usuario si el archivo no es valido.
    /// </summary>
    public string? Guardar(IFormFile? archivo, string carpeta, string? urlAnterior)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Formato de imagen no permitido. Formatos aceptados: {string.Join(", ", ExtensionesPermitidas)}.");
        }

        if (archivo.Length > TamanoMaximo)
        {
            throw new InvalidOperationException("La imagen supera el maximo de 5 MB.");
        }

        if (!string.Equals(archivo.ContentType?.Split('/')[0], "image", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El archivo no es una imagen.");
        }

        var wwwroot = _entorno.WebRootPath;
        if (string.IsNullOrEmpty(wwwroot))
        {
            wwwroot = Path.Combine(_entorno.ContentRootPath, "wwwroot");
        }

        var rutaCarpeta = Path.Combine(wwwroot, "Uploads", carpeta);
        Directory.CreateDirectory(rutaCarpeta);

        var nombre = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(rutaCarpeta, nombre);

        using (var flujo = File.Create(rutaCompleta))
        {
            archivo.CopyTo(flujo);
        }

        _logger.LogInformation("Imagen guardada en {Ruta}", rutaCompleta);

        // Si habia una imagen local previa (uploads propios, no urls externas) se borra.
        BorrarSiLocal(carpeta, urlAnterior);

        return $"/Uploads/{carpeta}/{nombre}";
    }

    /// <summary>
    /// Elimina el archivo local de una imagen si la url apunta a Uploads (y no es una
    /// url externa). Se usa al reemplazar o quitar un avatar/portada.
    /// </summary>
    public void BorrarSiLocal(string carpeta, string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith($"/Uploads/{carpeta}/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var wwwroot = _entorno.WebRootPath;
        if (string.IsNullOrEmpty(wwwroot))
        {
            wwwroot = Path.Combine(_entorno.ContentRootPath, "wwwroot");
        }

        // Path.GetFileName evita que una url maliciosa salga de la carpeta (path traversal).
        var archivo = Path.GetFileName(url);
        var rutaCompleta = Path.Combine(wwwroot, "Uploads", carpeta, archivo);

        try
        {
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
                _logger.LogInformation("Imagen anterior borrada: {Ruta}", rutaCompleta);
            }
        }
        catch (Exception ex)
        {
            // Si falla el borrado fisico no rompemos la operacion; solo lo registramos.
            _logger.LogWarning(ex, "No se pudo borrar la imagen anterior {Ruta}", rutaCompleta);
        }
    }
}
