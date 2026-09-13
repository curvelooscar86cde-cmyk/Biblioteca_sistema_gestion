using Biblioteca.Dominio;

namespace Biblioteca.LogicaNegocio.Contratos;

public interface IServicioPrestamo
{
    Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync();
    Task<IReadOnlyList<Prestamo>> ObtenerPorUsuarioAsync(int idUsuario);
    Task<int> RegistrarPrestamoAsync(int idUsuario, IReadOnlyDictionary<int, int> librosSolicitados, DateTime fechaDevolucionEsperada);
    Task RegistrarDevolucionAsync(int idPrestamo);
}
