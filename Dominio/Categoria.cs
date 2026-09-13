namespace Biblioteca.Dominio;

public class Categoria
{
    public int IdCategoria { get; private set; }
    public string Nombre { get; private set; }

    public Categoria(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la categoría es obligatorio.", nameof(nombre));

        Nombre = nombre;
    }

    public Categoria(int idCategoria, string nombre) : this(nombre)
    {
        IdCategoria = idCategoria;
    }
}
