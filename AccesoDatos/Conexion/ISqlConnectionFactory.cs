using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Conexion;

public interface ISqlConnectionFactory
{
    SqlConnection CrearConexionAbierta();
}
