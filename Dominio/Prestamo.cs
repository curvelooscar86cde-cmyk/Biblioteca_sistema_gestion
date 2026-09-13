namespace Biblioteca.Dominio;

public class Prestamo
{
    private readonly List<DetallePrestamo> _detalles = [];

    public int IdPrestamo { get; private set; }
    public Usuario Usuario { get; private set; }
    public DateTime FechaPrestamo { get; private set; }
    public DateTime FechaDevolucionEsperada { get; private set; }
    public EstadoPrestamo Estado { get; private set; }
    public IReadOnlyList<DetallePrestamo> Detalles => _detalles.AsReadOnly();

    public Prestamo(Usuario usuario, DateTime fechaPrestamo, DateTime fechaDevolucionEsperada)
    {
        if (fechaDevolucionEsperada <= fechaPrestamo)
            throw new ArgumentException("La fecha de devolución esperada debe ser posterior a la fecha del préstamo.", nameof(fechaDevolucionEsperada));

        Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
        FechaPrestamo = fechaPrestamo;
        FechaDevolucionEsperada = fechaDevolucionEsperada;
        Estado = EstadoPrestamo.Activo;
    }

    public Prestamo(int idPrestamo, Usuario usuario, DateTime fechaPrestamo, DateTime fechaDevolucionEsperada, EstadoPrestamo estado)
        : this(usuario, fechaPrestamo, fechaDevolucionEsperada)
    {
        IdPrestamo = idPrestamo;
        Estado = estado;
    }

    public void AgregarDetalle(DetallePrestamo detalle)
    {
        if (Estado != EstadoPrestamo.Activo)
            throw new InvalidOperationException("No se pueden agregar libros a un préstamo que ya fue cerrado.");

        _detalles.Add(detalle ?? throw new ArgumentNullException(nameof(detalle)));
    }

    internal void CargarDetalleExistente(DetallePrestamo detalle) =>
        _detalles.Add(detalle ?? throw new ArgumentNullException(nameof(detalle)));

    public void RegistrarDevolucion()
    {
        if (Estado != EstadoPrestamo.Activo && Estado != EstadoPrestamo.Atrasado)
            throw new InvalidOperationException("El préstamo ya fue devuelto anteriormente.");

        foreach (var detalle in _detalles)
            detalle.Libro.AumentarDisponible();

        Estado = EstadoPrestamo.Devuelto;
    }

    public bool EstaAtrasado(DateTime fechaActual) =>
        Estado == EstadoPrestamo.Activo && fechaActual > FechaDevolucionEsperada;
}
