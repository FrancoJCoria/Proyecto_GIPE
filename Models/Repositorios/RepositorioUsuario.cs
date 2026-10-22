using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.Enums;
using Proyecto_GIPE.Models.IRepositorios;

namespace Proyecto_GIPE.Models.Repositorios;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    public RepositorioUsuario(IConfiguration configuration)
        : base(configuration)
    {
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
        Console.WriteLine($"Error al obtener los usuarios: {e.Message}");
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
        Console.WriteLine($"Error al obtener el usuario por email: {e.Message}");
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
            Console.WriteLine($"Error al obtener el usario por ID {e.Message}");
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

        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 20).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@PasswordHash", MySqlDbType.VarChar, 255).Value = usuario.PasswordHash;
        comando.Parameters.Add("@Rol", MySqlDbType.Int32).Value = (int)usuario.Rol;
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;
        comando.Parameters.Add("@Url_avatar", MySqlDbType.VarChar, 500).Value = usuario.Url_avatar;

        conexion.Open();

        return Convert.ToInt32(comando.ExecuteScalar());
        }catch(Exception e){
            Console.WriteLine($"Error al crear el usuario: {e.Message}");
            return 0;
        }
    }


    //------------------------------------------------------------ ACTUALIZAR EL USUARIO --------------------------------------------------------

    public bool ActualizarUsuario(Usuario usuario)
    {
        using var conexion = new MySqlConnection(connectionString);
        try{
        string consultaSql = @"UPDATE usuario
        SET nombre = @Nombre, email = @Email, url_avatar = @Url_avatar, rol = @Rol,estado = @Estado
        WHERE id = @Id;";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = usuario.Id;
        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 20).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@Url_avatar", MySqlDbType.VarChar, 500).Value =
        (object?)usuario.Url_avatar ?? DBNull.Value;       
        comando.Parameters.Add("@Rol", MySqlDbType.Int32).Value = (int)usuario.Rol;
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;
        conexion.Open();

        return comando.ExecuteNonQuery() > 0;
        }catch(Exception e){
            Console.WriteLine($"Error al actualizar el usuario: {e.Message}");
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
            Console.WriteLine($"Error al cambiar el estado de usuario: {e.Message}");
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
            Console.WriteLine($"Error al modificar el avatar de usuario {e.Message}");
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
