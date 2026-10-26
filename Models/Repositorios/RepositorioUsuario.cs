using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    private readonly ILogger<RepositorioUsuario> _logger;

    public RepositorioUsuario(
        IConfiguration configuration,
        ILogger<RepositorioUsuario> logger)
        : base(configuration)
    {
        _logger = logger;
    }

//------------------------------------------------------------OBTENER TODOS LOS USUARIOS LIST --------------------------------------------------------
    public IList<Usuario> ObtenerTodos()
    {
    
    var lista = new List<Usuario>();

    try
    {
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id, nombre, email, password_hash, rol, estado, url_avatar
        FROM usuario;";

        using var comando = new MySqlCommand(consultaSql, conexion);
        conexion.Open();

        using var lector = comando.ExecuteReader();

        while (lector.Read())
        {
            lista.Add(LeerUsuario(lector));
        }

        return lista;
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error al obtener los usuarios");
        return lista;
    }
    }
    //------------------------------------------------------------OBTENER USUARIO POR EMAIL --------------------------------------------------------
    public Usuario? ObtenerPorEmail(string email)
    {
    try
    {
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"SELECT id, nombre, email, password_hash, rol, estado, url_avatar
        FROM usuario
        WHERE email = @Email
        LIMIT 1;";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = email;

        conexion.Open();

        using var lector = comando.ExecuteReader();

        if (lector.Read())
        {
            return LeerUsuario(lector);
        }

        return null;
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error al obtener el usuario por email: {Email}", email);
        return null;
    }
    }

    public Usuario? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);
            var consulta = @"SELECT id, nombre, email, password_hash, rol, estado, url_avatar
            FROM usuario 
            WHERE id = @Id
            LIMIT 1;";

            using var comando = new MySqlCommand(consulta, conexion);
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            conexion.Open();

            using var lector = comando.ExecuteReader();

            if (lector.Read())
            {
                return LeerUsuario(lector);
            }

            return null;
        }catch(Exception e)
        {
            _logger.LogError(e, "Error al obtener el usuario por ID {Id}", id);
            return null;
        }
    }
    //------------------------------------------------------------CREAR USUARIO --------------------------------------------------------
    public int CrearUsuario(Usuario usuario)
    {
        try{
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"INSERT INTO usuario(nombre, email, password_hash, rol, estado, url_avatar)
        VALUES (@Nombre, @Email, @PasswordHash, @Rol, @Estado, @Url_avatar);
        SELECT LAST_INSERT_ID();";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@PasswordHash", MySqlDbType.VarChar, 255).Value = usuario.PasswordHash;
        comando.Parameters.Add("@Rol", MySqlDbType.VarChar, 20).Value = usuario.Rol.ToString();
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;
        comando.Parameters.Add("@Url_avatar", MySqlDbType.VarChar, 500).Value = usuario.Url_avatar;

        conexion.Open();

        return Convert.ToInt32(comando.ExecuteScalar());
        }catch(Exception e){
            _logger.LogError(e, "Error al crear el usuario {Email}", usuario.Email);
            return 0;
        }
    }


    //------------------------------------------------------------ ACTUALIZAR EL USUARIO --------------------------------------------------------

    public bool ActualizarUsuario(Usuario usuario)
    {
        using var conexion = new MySqlConnection(connectionString);
        try{
        string consultaSql = @"UPDATE usuario
        SET nombre = @Nombre, email = @Email, url_avatar = @Url_avatar, rol = @Rol, estado = @Estado,
            password_hash = COALESCE(NULLIF(@PasswordHash, ''), password_hash)
        WHERE id = @Id;";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = usuario.Id;
        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 150).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@Url_avatar", MySqlDbType.VarChar, 500).Value =
        (object?)usuario.Url_avatar ?? DBNull.Value;       
        comando.Parameters.Add("@Rol", MySqlDbType.VarChar, 20).Value = usuario.Rol.ToString();
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;
        comando.Parameters.Add("@PasswordHash", MySqlDbType.VarChar, 255).Value = usuario.PasswordHash;
        conexion.Open();

        return comando.ExecuteNonQuery() > 0;
        }catch(Exception e){
            _logger.LogError(e, "Error al actualizar el usuario {Email}", usuario.Email);
            return false;
        }
    }


    //------------------------------------------------------------ ACTUALIZAR ESTADO DE USUARIO --------------------------------------------------------

    public bool CambiarEstadoUsuario(int id, bool nuevoEstado)
    {
        try{
        using var conexion = new MySqlConnection(connectionString);
        string consultaSql = @"UPDATE usuario
        SET estado = @Estado
        WHERE id = @Id;";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = nuevoEstado;

        conexion.Open();

        return comando.ExecuteNonQuery() > 0;
        }catch(Exception e)
        {
            _logger.LogError(e, "Error al cambiar el estado del usuario {Id}", id);
            return false;
        }
    }


//------------------------------------------------------------ ACTUALIZAR EL AVATAR DE USUARIO --------------------------------------------------------

    public string? EditarAvatarUsuario(int id, string url)
    {
        try
        {
            using var conexion = new MySqlConnection(connectionString);

            string consulta = @"UPDATE usuario
            SET url_avatar = @url_avatar
            WHERE estado = 1 
            AND id = @Id;";

            using var comando = new MySqlCommand(consulta, conexion);
            comando.Parameters.Add("@url_avatar", MySqlDbType.VarChar,500).Value = url;
            comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
            conexion.Open();

            int filasAfectadas = comando.ExecuteNonQuery();

            if (filasAfectadas > 0)
            {
                return url;
            }

            return null;

        }catch(Exception e)
        {
            _logger.LogError(e, "Error al modificar el avatar del usuario {Id}", id);
            return null;
        }
    }
//--------------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static Usuario LeerUsuario(MySqlDataReader lector)
    {
        int indiceAvatar = lector.GetOrdinal("url_avatar");

        return new Usuario
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Email = lector.GetString("email"),
            PasswordHash = lector.GetString("password_hash"),
            Rol = Enum.Parse<RolUsuario>(lector.GetString("rol"), true),
            Estado = lector.GetBoolean("estado"),
            Url_avatar = lector.IsDBNull(indiceAvatar)
            ? null
            : lector.GetString(indiceAvatar)
        };
    }
}
