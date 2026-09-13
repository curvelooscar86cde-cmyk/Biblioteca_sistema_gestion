using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Conexion;

public class SqlConnectionFactory(string cadenaConexion) : ISqlConnectionFactory
{
    private readonly string _cadenaConexion = string.IsNullOrWhiteSpace(cadenaConexion)
        ? throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(cadenaConexion))
        : cadenaConexion;

    public SqlConnection CrearConexionAbierta()
    {
        var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();
        return conexion;
    }
}
