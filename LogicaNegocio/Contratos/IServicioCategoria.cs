using Biblioteca.Dominio;

namespace Biblioteca.LogicaNegocio.Contratos;

public interface IServicioCategoria
{
    Task<IReadOnlyList<Categoria>> ObtenerTodasAsync();
    Task<int> RegistrarAsync(Categoria categoria);
    Task ActualizarAsync(Categoria categoria);
    Task EliminarAsync(int idCategoria);
}
