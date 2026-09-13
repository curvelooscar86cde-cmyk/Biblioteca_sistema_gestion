using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Repositorios;

public class UsuarioRepository(ISqlConnectionFactory conexionFactory) : IUsuarioRepository
{
    private readonly ISqlConnectionFactory _conexionFactory = conexionFactory ?? throw new ArgumentNullException(nameof(conexionFactory));

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync()
    {
        var usuarios = new List<Usuario>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Usuario, Documento, Nombre, Apellidos, Telefono, Correo, Programa_Academico FROM Usuarios", conexion);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            usuarios.Add(MapearUsuario(lector));
        return usuarios;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int idUsuario)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Usuario, Documento, Nombre, Apellidos, Telefono, Correo, Programa_Academico FROM Usuarios WHERE ID_Usuario = @IdUsuario", conexion);
        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
        using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? MapearUsuario(lector) : null;
    }

    public async Task<bool> ExisteDocumentoAsync(string documento)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("SELECT COUNT(1) FROM Usuarios WHERE Documento = @Documento", conexion);
        comando.Parameters.AddWithValue("@Documento", documento);
        var cantidad = Convert.ToInt32(await comando.ExecuteScalarAsync());
        return cantidad > 0;
    }

    public async Task<int> RegistrarAsync(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            INSERT INTO Usuarios (Documento, Nombre, Apellidos, Telefono, Correo, Programa_Academico)
            OUTPUT INSERTED.ID_Usuario
            VALUES (@Documento, @Nombre, @Apellidos, @Telefono, @Correo, @ProgramaAcademico)
            """, conexion);
        AgregarParametros(comando, usuario);
        var idGenerado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(idGenerado);
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            """
            UPDATE Usuarios
            SET Documento = @Documento, Nombre = @Nombre, Apellidos = @Apellidos,
                Telefono = @Telefono, Correo = @Correo, Programa_Academico = @ProgramaAcademico
            WHERE ID_Usuario = @IdUsuario
            """, conexion);
        AgregarParametros(comando, usuario);
        comando.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un usuario con el id {usuario.IdUsuario}.");
    }

    public async Task EliminarAsync(int idUsuario)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("DELETE FROM Usuarios WHERE ID_Usuario = @IdUsuario", conexion);
        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe un usuario con el id {idUsuario}.");
    }

    private static void AgregarParametros(SqlCommand comando, Usuario usuario)
    {
        comando.Parameters.AddWithValue("@Documento", usuario.Documento);
        comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
        comando.Parameters.AddWithValue("@Apellidos", usuario.Apellidos);
        comando.Parameters.AddWithValue("@Telefono", usuario.Telefono);
        comando.Parameters.AddWithValue("@Correo", usuario.Correo);
        comando.Parameters.AddWithValue("@ProgramaAcademico", usuario.ProgramaAcademico);
    }

    private static Usuario MapearUsuario(SqlDataReader lector) =>
        new(
            lector.GetInt32(lector.GetOrdinal("ID_Usuario")),
            lector.GetString(lector.GetOrdinal("Documento")),
            lector.GetString(lector.GetOrdinal("Nombre")),
            lector.GetString(lector.GetOrdinal("Apellidos")),
            lector.GetString(lector.GetOrdinal("Telefono")),
            lector.GetString(lector.GetOrdinal("Correo")),
            lector.GetString(lector.GetOrdinal("Programa_Academico")));
}
