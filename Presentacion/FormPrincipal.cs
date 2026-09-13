using Biblioteca.LogicaNegocio.Contratos;

namespace Biblioteca.Presentacion;

public class FormPrincipal : Form
{
    private readonly IServicioAutor _servicioAutor;
    private readonly IServicioCategoria _servicioCategoria;
    private readonly IServicioLibro _servicioLibro;
    private readonly IServicioUsuario _servicioUsuario;
    private readonly IServicioPrestamo _servicioPrestamo;

    public FormPrincipal(
        IServicioAutor servicioAutor,
        IServicioCategoria servicioCategoria,
        IServicioLibro servicioLibro,
        IServicioUsuario servicioUsuario,
        IServicioPrestamo servicioPrestamo)
    {
        _servicioAutor = servicioAutor ?? throw new ArgumentNullException(nameof(servicioAutor));
        _servicioCategoria = servicioCategoria ?? throw new ArgumentNullException(nameof(servicioCategoria));
        _servicioLibro = servicioLibro ?? throw new ArgumentNullException(nameof(servicioLibro));
        _servicioUsuario = servicioUsuario ?? throw new ArgumentNullException(nameof(servicioUsuario));
        _servicioPrestamo = servicioPrestamo ?? throw new ArgumentNullException(nameof(servicioPrestamo));

        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Sistema de Gestión de Biblioteca";
        Width = 420;
        Height = 420;
        StartPosition = FormStartPosition.CenterScreen;

        var botonAutores = CrearBotonMenu("Gestión de Autores", 30);
        botonAutores.Click += (_, _) => new FormAutores(_servicioAutor).ShowDialog(this);

        var botonCategorias = CrearBotonMenu("Gestión de Categorías", 80);
        botonCategorias.Click += (_, _) => new FormCategorias(_servicioCategoria).ShowDialog(this);

        var botonLibros = CrearBotonMenu("Gestión de Libros", 130);
        botonLibros.Click += (_, _) => new FormLibros(_servicioLibro, _servicioAutor, _servicioCategoria).ShowDialog(this);

        var botonUsuarios = CrearBotonMenu("Gestión de Usuarios", 180);
        botonUsuarios.Click += (_, _) => new FormUsuarios(_servicioUsuario).ShowDialog(this);

        var botonPrestamos = CrearBotonMenu("Préstamos y Devoluciones", 230);
        botonPrestamos.Click += (_, _) => new FormPrestamos(_servicioPrestamo, _servicioUsuario, _servicioLibro).ShowDialog(this);

        var botonConsultas = CrearBotonMenu("Consultas", 280);
        botonConsultas.Click += (_, _) => new FormConsultas(_servicioLibro, _servicioUsuario, _servicioPrestamo, _servicioCategoria).ShowDialog(this);

        Controls.AddRange([botonAutores, botonCategorias, botonLibros, botonUsuarios, botonPrestamos, botonConsultas]);
    }

    private static Button CrearBotonMenu(string texto, int posicionVertical) =>
        new()
        {
            Text = texto,
            Left = 80,
            Top = posicionVertical,
            Width = 250,
            Height = 35
        };
}
