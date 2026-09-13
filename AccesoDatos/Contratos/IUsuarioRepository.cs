using Biblioteca.Dominio;

namespace Biblioteca.AccesoDatos.Contratos;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> ObtenerPorIdAsync(int idUsuario);
    Task<bool> ExisteDocumentoAsync(string documento);
    Task<int> RegistrarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task EliminarAsync(int idUsuario);
}
