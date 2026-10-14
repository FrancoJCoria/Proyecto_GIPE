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

        string consultaSql = @"SELECT id, nombre, email, password_hash, rol, estado
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

        string consultaSql = @"SELECT id, nombre, email, password_hash, rol, estado
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


    //------------------------------------------------------------CREAR USUARIO --------------------------------------------------------
    public int CrearUsuario(Usuario usuario)
    {
        try{
        using var conexion = new MySqlConnection(connectionString);

        string consultaSql = @"INSERT INTO usuario(nombre, email, password_hash, rol, estado)
        VALUES (@Nombre, @Email, @PasswordHash, @Rol, @Estado);
        SELECT LAST_INSERT_ID();";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 20).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@PasswordHash", MySqlDbType.VarChar, 255).Value = usuario.PasswordHash;
        comando.Parameters.Add("@Rol", MySqlDbType.Int32).Value = (int)usuario.Rol;
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;

        conexion.Open();

        return Convert.ToInt32(comando.ExecuteScalar());
        }catch(Exception e){
            Console.WriteLine($"Error al crear el usuario: {e.Message}");
            return 0;
        }
    }


    //------------------------------------------------------------ ACTUALIZAR ESTADO DE USUARIO --------------------------------------------------------

    public bool ActualizarUsuario(Usuario usuario)
    {
        using var conexion = new MySqlConnection(connectionString);
        try{
        string consultaSql = @"UPDATE usuario
        SET nombre = @Nombre, email = @Email, password_hash = @PasswordHash,rol = @Rol,estado = @Estado
        WHERE id = @Id;";

        using var comando = new MySqlCommand(consultaSql, conexion);

        comando.Parameters.Add("@Id", MySqlDbType.Int32).Value = usuario.Id;
        comando.Parameters.Add("@Nombre", MySqlDbType.VarChar, 20).Value = usuario.Nombre;
        comando.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = usuario.Email;
        comando.Parameters.Add("@PasswordHash", MySqlDbType.VarChar, 255).Value = usuario.PasswordHash;
        comando.Parameters.Add("@Rol", MySqlDbType.Int32).Value = (int)usuario.Rol;
        comando.Parameters.Add("@Estado", MySqlDbType.Bit).Value = usuario.Estado;
        conexion.Open();

        return comando.ExecuteNonQuery() > 0;
        }catch(Exception e){
            Console.WriteLine($"Error al actualizar el usuario: {e.Message}");
            return false;
        }
    }

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

//--------------------------------------------------------------------FUNCIONES --------------------------------------------------------
    private static Usuario LeerUsuario(MySqlDataReader lector)
    {
        return new Usuario
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Email = lector.GetString("email"),
            PasswordHash = lector.GetString("password_hash"),
            Rol = (RolUsuario)lector.GetInt32("rol"),
            Estado = lector.GetBoolean("estado")
        };
    }
}
