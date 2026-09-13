using Biblioteca.Dominio;

namespace Biblioteca.LogicaNegocio.Contratos;

public interface IServicioAutor
{
    Task<IReadOnlyList<Autor>> ObtenerTodosAsync();
    Task<int> RegistrarAsync(Autor autor);
    Task ActualizarAsync(Autor autor);
    Task EliminarAsync(int idAutor);
}
