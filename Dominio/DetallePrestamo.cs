namespace Biblioteca.Dominio;

public class DetallePrestamo
{
    public int IdDetalle { get; private set; }
    public Libro Libro { get; private set; }
    public int Cantidad { get; private set; }

    public DetallePrestamo(Libro libro, int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");

        Libro = libro ?? throw new ArgumentNullException(nameof(libro));
        Cantidad = cantidad;
    }

    public DetallePrestamo(int idDetalle, Libro libro, int cantidad) : this(libro, cantidad)
    {
        IdDetalle = idDetalle;
    }
}
