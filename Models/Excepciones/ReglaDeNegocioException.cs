namespace Proyecto_GIPE.Models.Excepciones;

/// <summary>
/// Representa una validacion de negocio incumplida (por ejemplo, un producto inexistente
/// o inactivo). El mensaje esta pensado para mostrarse al usuario.
/// </summary>
public class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string mensaje) : base(mensaje)
    {
    }
}
