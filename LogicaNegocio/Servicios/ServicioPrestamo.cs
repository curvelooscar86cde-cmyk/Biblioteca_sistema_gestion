using Biblioteca.AccesoDatos.Contratos;
using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.LogicaNegocio.Servicios;

public class ServicioPrestamo(
    IPrestamoRepository prestamoRepository,
    IUsuarioRepository usuarioRepository,
    ILibroRepository libroRepository) : IServicioPrestamo
{
    private readonly IPrestamoRepository _prestamoRepository = prestamoRepository ?? throw new ArgumentNullException(nameof(prestamoRepository));
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    private readonly ILibroRepository _libroRepository = libroRepository ?? throw new ArgumentNullException(nameof(libroRepository));

    public Task<IReadOnlyList<Prestamo>> ObtenerTodosAsync() => _prestamoRepository.ObtenerTodosAsync();

    public Task<IReadOnlyList<Prestamo>> ObtenerPorUsuarioAsync(int idUsuario) => _prestamoRepository.ObtenerPorUsuarioAsync(idUsuario);

    public async Task<int> RegistrarPrestamoAsync(int idUsuario, IReadOnlyDictionary<int, int> librosSolicitados, DateTime fechaDevolucionEsperada)
    {
        if (librosSolicitados.Count == 0)
            throw new ArgumentException("Debes seleccionar al menos un libro para el préstamo.", nameof(librosSolicitados));

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario)
            ?? throw new EntidadNoEncontradaException($"No existe un usuario con el id {idUsuario}.");

        var prestamo = new Prestamo(usuario, DateTime.Now, fechaDevolucionEsperada);

        foreach (var (idLibro, cantidadSolicitada) in librosSolicitados)
        {
            var libro = await _libroRepository.ObtenerPorIdAsync(idLibro)
                ?? throw new EntidadNoEncontradaException($"No existe un libro con el id {idLibro}.");

            if (libro.Disponible < cantidadSolicitada)
                throw new LibroNoDisponibleException(
                    $"El libro '{libro.Titulo}' no tiene existencias suficientes (disponibles: {libro.Disponible}, solicitados: {cantidadSolicitada}).");

            prestamo.AgregarDetalle(new DetallePrestamo(libro, cantidadSolicitada));
        }

        return await _prestamoRepository.RegistrarAsync(prestamo);
    }

    public async Task RegistrarDevolucionAsync(int idPrestamo)
    {
        var prestamo = await _prestamoRepository.ObtenerPorIdAsync(idPrestamo)
            ?? throw new EntidadNoEncontradaException($"No existe un préstamo con el id {idPrestamo}.");

        prestamo.RegistrarDevolucion();
        await _prestamoRepository.ActualizarEstadoAsync(prestamo);
    }
}
