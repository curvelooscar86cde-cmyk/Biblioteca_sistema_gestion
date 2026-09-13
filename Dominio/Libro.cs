namespace Biblioteca.Dominio;

public class Libro
{
    public int IdLibro { get; private set; }
    public string Codigo { get; private set; }
    public string Titulo { get; private set; }
    public string Isbn { get; private set; }
    public int Cantidad { get; private set; }
    public int Disponible { get; private set; }
    public Autor Autor { get; private set; }
    public Categoria Categoria { get; private set; }

    public Libro(string codigo, string titulo, string isbn, int cantidad, Autor autor, Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del libro es obligatorio.", nameof(codigo));

        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del libro es obligatorio.", nameof(titulo));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("El ISBN del libro es obligatorio.", nameof(isbn));

        if (cantidad <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");

        Codigo = codigo;
        Titulo = titulo;
        Isbn = isbn;
        Cantidad = cantidad;
        Disponible = cantidad;
        Autor = autor ?? throw new ArgumentNullException(nameof(autor));
        Categoria = categoria ?? throw new ArgumentNullException(nameof(categoria));
    }

    public Libro(int idLibro, string codigo, string titulo, string isbn, int cantidad, int disponible, Autor autor, Categoria categoria)
        : this(codigo, titulo, isbn, cantidad, autor, categoria)
    {
        if (disponible < 0 || disponible > cantidad)
            throw new ArgumentOutOfRangeException(nameof(disponible), "La cantidad disponible no puede ser negativa ni mayor que la cantidad total.");

        IdLibro = idLibro;
        Disponible = disponible;
    }

    public bool HayDisponibilidad() => Disponible > 0;

    public void DisminuirDisponible()
    {
        if (!HayDisponibilidad())
            throw new InvalidOperationException($"El libro '{Titulo}' no tiene ejemplares disponibles.");

        Disponible--;
    }

    public void AumentarDisponible()
    {
        if (Disponible >= Cantidad)
            throw new InvalidOperationException($"El libro '{Titulo}' ya tiene todos sus ejemplares disponibles.");

        Disponible++;
    }
}
