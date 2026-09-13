namespace Biblioteca.Dominio;

public class Autor
{
    public int IdAutor { get; private set; }
    public string Nombre { get; private set; }
    public string Apellidos { get; private set; }
    public string Nacionalidad { get; private set; }
    public DateTime FechaNacimiento { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    public Autor(string nombre, string apellidos, string nacionalidad, DateTime fechaNacimiento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del autor es obligatorio.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ArgumentException("Los apellidos del autor son obligatorios.", nameof(apellidos));

        if (string.IsNullOrWhiteSpace(nacionalidad))
            throw new ArgumentException("La nacionalidad del autor es obligatoria.", nameof(nacionalidad));

        Nombre = nombre;
        Apellidos = apellidos;
        Nacionalidad = nacionalidad;
        FechaNacimiento = fechaNacimiento;
    }

    public Autor(int idAutor, string nombre, string apellidos, string nacionalidad, DateTime fechaNacimiento)
        : this(nombre, apellidos, nacionalidad, fechaNacimiento)
    {
        IdAutor = idAutor;
    }
}
