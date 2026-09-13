using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;

namespace Biblioteca.Presentacion;

public class FormConsultas : Form
{
    private sealed record LibroFila(string Codigo, string Titulo, string Autor, string Categoria, int Cantidad, int Disponible);
    private sealed record UsuarioConPrestamosFila(string Documento, string NombreCompleto, int CantidadPrestamos);
    private sealed record HistorialFila(int IdPrestamo, string Usuario, DateTime FechaPrestamo, DateTime FechaDevolucionEsperada, string Estado, string Libros);
    private sealed record CategoriaConteoFila(string Categoria, int TitulosDistintos, int TotalEjemplares);
    private sealed record TotalFila(string Descripcion, int Valor);

    private readonly IServicioLibro _servicioLibro;
    private readonly IServicioUsuario _servicioUsuario;
    private readonly IServicioPrestamo _servicioPrestamo;
    private readonly IServicioCategoria _servicioCategoria;

    private readonly Label _etiquetaTitulo = new();
    private readonly DataGridView _grilla = new();

    public FormConsultas(
        IServicioLibro servicioLibro,
        IServicioUsuario servicioUsuario,
        IServicioPrestamo servicioPrestamo,
        IServicioCategoria servicioCategoria)
    {
        _servicioLibro = servicioLibro ?? throw new ArgumentNullException(nameof(servicioLibro));
        _servicioUsuario = servicioUsuario ?? throw new ArgumentNullException(nameof(servicioUsuario));
        _servicioPrestamo = servicioPrestamo ?? throw new ArgumentNullException(nameof(servicioPrestamo));
        _servicioCategoria = servicioCategoria ?? throw new ArgumentNullException(nameof(servicioCategoria));

        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Consultas";
        Width = 820;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        var botonDisponibles = CrearBotonConsulta("Libros disponibles", 20);
        botonDisponibles.Click += async (_, _) => await MostrarLibrosDisponiblesAsync();

        var botonPrestados = CrearBotonConsulta("Libros prestados", 60);
        botonPrestados.Click += async (_, _) => await MostrarLibrosPrestadosAsync();

        var botonUsuarios = CrearBotonConsulta("Usuarios con préstamos", 100);
        botonUsuarios.Click += async (_, _) => await MostrarUsuariosConPrestamosAsync();

        var botonHistorial = CrearBotonConsulta("Historial de préstamos", 140);
        botonHistorial.Click += async (_, _) => await MostrarHistorialPrestamosAsync();

        var botonCategorias = CrearBotonConsulta("Libros por categoría", 180);
        botonCategorias.Click += async (_, _) => await MostrarLibrosPorCategoriaAsync();

        var botonTotalPrestados = CrearBotonConsulta("Total libros prestados", 220);
        botonTotalPrestados.Click += async (_, _) => await MostrarTotalLibrosPrestadosAsync();

        _etiquetaTitulo.Left = 220; _etiquetaTitulo.Top = 20; _etiquetaTitulo.Width = 550; _etiquetaTitulo.Height = 30;
        _etiquetaTitulo.Font = new Font(_etiquetaTitulo.Font, FontStyle.Bold);
        _etiquetaTitulo.Text = "Selecciona una consulta del panel izquierdo.";

        _grilla.Left = 220; _grilla.Top = 60; _grilla.Width = 570; _grilla.Height = 480;
        _grilla.ReadOnly = true;
        _grilla.AllowUserToAddRows = false;
        _grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        Controls.AddRange([
            botonDisponibles, botonPrestados, botonUsuarios, botonHistorial, botonCategorias, botonTotalPrestados,
            _etiquetaTitulo, _grilla
        ]);
    }

    private static Button CrearBotonConsulta(string texto, int posicionVertical) =>
        new()
        {
            Text = texto,
            Left = 20,
            Top = posicionVertical,
            Width = 180,
            Height = 35
        };

    private async Task MostrarLibrosDisponiblesAsync()
    {
        var libros = await _servicioLibro.ObtenerTodosAsync();
        MostrarResultado(
            "Libros disponibles (con al menos 1 ejemplar libre)",
            libros.Where(libro => libro.Disponible > 0).Select(ProyectarLibro).ToList());
    }

    private async Task MostrarLibrosPrestadosAsync()
    {
        var libros = await _servicioLibro.ObtenerTodosAsync();
        MostrarResultado(
            "Libros con al menos un ejemplar actualmente prestado",
            libros.Where(libro => libro.Disponible < libro.Cantidad).Select(ProyectarLibro).ToList());
    }

    private async Task MostrarUsuariosConPrestamosAsync()
    {
        var usuarios = await _servicioUsuario.ObtenerTodosAsync();
        var prestamos = await _servicioPrestamo.ObtenerTodosAsync();

        var conteoPorUsuario = prestamos
            .GroupBy(prestamo => prestamo.Usuario.IdUsuario)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());

        var filas = usuarios
            .Where(usuario => conteoPorUsuario.ContainsKey(usuario.IdUsuario))
            .Select(usuario => new UsuarioConPrestamosFila(usuario.Documento, usuario.NombreCompleto, conteoPorUsuario[usuario.IdUsuario]))
            .OrderByDescending(fila => fila.CantidadPrestamos)
            .ToList();

        MostrarResultado("Usuarios que tienen al menos un préstamo registrado", filas);
    }

    private async Task MostrarHistorialPrestamosAsync()
    {
        var prestamos = await _servicioPrestamo.ObtenerTodosAsync();
        var filas = prestamos
            .OrderByDescending(prestamo => prestamo.FechaPrestamo)
            .Select(prestamo => new HistorialFila(
                prestamo.IdPrestamo,
                prestamo.Usuario.NombreCompleto,
                prestamo.FechaPrestamo,
                prestamo.FechaDevolucionEsperada,
                prestamo.Estado.ToString(),
                string.Join(", ", prestamo.Detalles.Select(detalle => $"{detalle.Libro.Titulo} x{detalle.Cantidad}"))))
            .ToList();

        MostrarResultado("Historial completo de préstamos", filas);
    }

    private async Task MostrarLibrosPorCategoriaAsync()
    {
        var categorias = await _servicioCategoria.ObtenerTodasAsync();
        var libros = await _servicioLibro.ObtenerTodosAsync();

        var filas = categorias
            .Select(categoria =>
            {
                var librosDeCategoria = libros.Where(libro => libro.Categoria.IdCategoria == categoria.IdCategoria).ToList();
                return new CategoriaConteoFila(categoria.Nombre, librosDeCategoria.Count, librosDeCategoria.Sum(libro => libro.Cantidad));
            })
            .OrderByDescending(fila => fila.TitulosDistintos)
            .ToList();

        MostrarResultado("Cantidad de libros (títulos y ejemplares) por categoría", filas);
    }

    private async Task MostrarTotalLibrosPrestadosAsync()
    {
        var libros = await _servicioLibro.ObtenerTodosAsync();
        var totalEjemplaresPrestados = libros.Sum(libro => libro.Cantidad - libro.Disponible);
        var totalTitulosConPrestamo = libros.Count(libro => libro.Disponible < libro.Cantidad);

        var filas = new List<TotalFila>
        {
            new("Ejemplares actualmente prestados", totalEjemplaresPrestados),
            new("Títulos distintos con al menos un ejemplar prestado", totalTitulosConPrestamo)
        };

        MostrarResultado("Totales de libros prestados", filas);
    }

    private static LibroFila ProyectarLibro(Libro libro) =>
        new(libro.Codigo, libro.Titulo, libro.Autor.NombreCompleto, libro.Categoria.Nombre, libro.Cantidad, libro.Disponible);

    private void MostrarResultado<T>(string titulo, List<T> filas)
    {
        _etiquetaTitulo.Text = titulo;
        _grilla.DataSource = filas;
    }
}
