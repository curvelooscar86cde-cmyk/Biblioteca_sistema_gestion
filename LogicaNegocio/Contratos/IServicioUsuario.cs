using Biblioteca.Dominio;

namespace Biblioteca.LogicaNegocio.Contratos;

public interface IServicioUsuario
{
    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync();
    Task<int> RegistrarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task EliminarAsync(int idUsuario);
}
