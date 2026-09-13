using Biblioteca.AccesoDatos.Conexion;
using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Microsoft.Data.SqlClient;

namespace Biblioteca.AccesoDatos.Repositorios;

public class CategoriaRepository(ISqlConnectionFactory conexionFactory) : ICategoriaRepository
{
    private readonly ISqlConnectionFactory _conexionFactory = conexionFactory ?? throw new ArgumentNullException(nameof(conexionFactory));

    public async Task<IReadOnlyList<Categoria>> ObtenerTodasAsync()
    {
        var categorias = new List<Categoria>();
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("SELECT ID_Categoria, Nombre FROM Categorias", conexion);
        using var lector = await comando.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            categorias.Add(MapearCategoria(lector));
        return categorias;
    }

    public async Task<Categoria?> ObtenerPorIdAsync(int idCategoria)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "SELECT ID_Categoria, Nombre FROM Categorias WHERE ID_Categoria = @IdCategoria", conexion);
        comando.Parameters.AddWithValue("@IdCategoria", idCategoria);
        using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? MapearCategoria(lector) : null;
    }

    public async Task<int> RegistrarAsync(Categoria categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "INSERT INTO Categorias (Nombre) OUTPUT INSERTED.ID_Categoria VALUES (@Nombre)", conexion);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
        var idGenerado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(idGenerado);
    }

    public async Task ActualizarAsync(Categoria categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand(
            "UPDATE Categorias SET Nombre = @Nombre WHERE ID_Categoria = @IdCategoria", conexion);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
        comando.Parameters.AddWithValue("@IdCategoria", categoria.IdCategoria);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe una categoría con el id {categoria.IdCategoria}.");
    }

    public async Task EliminarAsync(int idCategoria)
    {
        using var conexion = _conexionFactory.CrearConexionAbierta();
        using var comando = new SqlCommand("DELETE FROM Categorias WHERE ID_Categoria = @IdCategoria", conexion);
        comando.Parameters.AddWithValue("@IdCategoria", idCategoria);
        var filasAfectadas = await comando.ExecuteNonQueryAsync();
        if (filasAfectadas == 0)
            throw new InvalidOperationException($"No existe una categoría con el id {idCategoria}.");
    }

    private static Categoria MapearCategoria(SqlDataReader lector) =>
        new(lector.GetInt32(lector.GetOrdinal("ID_Categoria")), lector.GetString(lector.GetOrdinal("Nombre")));
}
