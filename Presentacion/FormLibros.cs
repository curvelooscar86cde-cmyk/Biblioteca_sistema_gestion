using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.Presentacion;

public class FormLibros : Form
{
    private sealed record LibroFila(int IdLibro, string Codigo, string Titulo, string Isbn, int Cantidad, int Disponible, string Autor, string Categoria);

    private sealed record OpcionCombo(int Id, string Descripcion)
    {
        public override string ToString() => Descripcion;
    }

    private readonly IServicioLibro _servicioLibro;
    private readonly IServicioAutor _servicioAutor;
    private readonly IServicioCategoria _servicioCategoria;

    private readonly Dictionary<int, Libro> _librosPorId = [];
    private readonly Dictionary<int, Autor> _autoresPorId = [];
    private readonly Dictionary<int, Categoria> _categoriasPorId = [];

    private readonly TextBox _campoCodigo = new();
    private readonly TextBox _campoTitulo = new();
    private readonly TextBox _campoIsbn = new();
    private readonly NumericUpDown _campoCantidad = new();
    private readonly ComboBox _comboAutor = new();
    private readonly ComboBox _comboCategoria = new();
    private readonly TextBox _campoBuscarTitulo = new();
    private readonly Button _botonBuscar = new();
    private readonly Button _botonRegistrar = new();
    private readonly Button _botonActualizar = new();
    private readonly Button _botonEliminar = new();
    private readonly Button _botonLimpiar = new();
    private readonly DataGridView _grilla = new();

    private int? _idLibroSeleccionado;

    public FormLibros(IServicioLibro servicioLibro, IServicioAutor servicioAutor, IServicioCategoria servicioCategoria)
    {
        _servicioLibro = servicioLibro ?? throw new ArgumentNullException(nameof(servicioLibro));
        _servicioAutor = servicioAutor ?? throw new ArgumentNullException(nameof(servicioAutor));
        _servicioCategoria = servicioCategoria ?? throw new ArgumentNullException(nameof(servicioCategoria));

        InicializarComponentes();
        Load += async (_, _) =>
        {
            await CargarAutoresYCategoriasAsync();
            await CargarLibrosAsync();
        };
    }

    private void InicializarComponentes()
    {
        Text = "Gestión de Libros";
        Width = 780;
        Height = 560;
        StartPosition = FormStartPosition.CenterParent;

        var etiquetaCodigo = new Label { Text = "Código:", Left = 20, Top = 20, Width = 80 };
        _campoCodigo.Left = 110; _campoCodigo.Top = 18; _campoCodigo.Width = 150;

        var etiquetaTitulo = new Label { Text = "Título:", Left = 20, Top = 55, Width = 80 };
        _campoTitulo.Left = 110; _campoTitulo.Top = 53; _campoTitulo.Width = 400;

        var etiquetaIsbn = new Label { Text = "ISBN:", Left = 20, Top = 90, Width = 80 };
        _campoIsbn.Left = 110; _campoIsbn.Top = 88; _campoIsbn.Width = 200;

        var etiquetaCantidad = new Label { Text = "Cantidad:", Left = 330, Top = 90, Width = 80 };
        _campoCantidad.Left = 420; _campoCantidad.Top = 88; _campoCantidad.Width = 90;
        _campoCantidad.Minimum = 1;
        _campoCantidad.Maximum = 1000;
        _campoCantidad.Value = 1;

        var etiquetaAutor = new Label { Text = "Autor:", Left = 20, Top = 125, Width = 80 };
        _comboAutor.Left = 110; _comboAutor.Top = 123; _comboAutor.Width = 300;
        _comboAutor.DropDownStyle = ComboBoxStyle.DropDownList;

        var etiquetaCategoria = new Label { Text = "Categoría:", Left = 20, Top = 160, Width = 80 };
        _comboCategoria.Left = 110; _comboCategoria.Top = 158; _comboCategoria.Width = 300;
        _comboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;

        _botonRegistrar.Text = "Registrar";
        _botonRegistrar.Left = 540; _botonRegistrar.Top = 18; _botonRegistrar.Width = 100;
        _botonRegistrar.Click += async (_, _) => await RegistrarAsync();

        _botonActualizar.Text = "Actualizar";
        _botonActualizar.Left = 650; _botonActualizar.Top = 18; _botonActualizar.Width = 100;
        _botonActualizar.Click += async (_, _) => await ActualizarAsync();

        _botonEliminar.Text = "Eliminar";
        _botonEliminar.Left = 540; _botonEliminar.Top = 53; _botonEliminar.Width = 100;
        _botonEliminar.Click += async (_, _) => await EliminarAsync();

        _botonLimpiar.Text = "Limpiar";
        _botonLimpiar.Left = 650; _botonLimpiar.Top = 53; _botonLimpiar.Width = 100;
        _botonLimpiar.Click += (_, _) => LimpiarFormulario();

        var etiquetaBuscar = new Label { Text = "Buscar por título:", Left = 20, Top = 210, Width = 110 };
        _campoBuscarTitulo.Left = 140; _campoBuscarTitulo.Top = 208; _campoBuscarTitulo.Width = 300;

        _botonBuscar.Text = "Buscar";
        _botonBuscar.Left = 450; _botonBuscar.Top = 206; _botonBuscar.Width = 100;
        _botonBuscar.Click += async (_, _) => await BuscarAsync();

        _grilla.Left = 20; _grilla.Top = 250; _grilla.Width = 720; _grilla.Height = 260;
        _grilla.ReadOnly = true;
        _grilla.AllowUserToAddRows = false;
        _grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grilla.MultiSelect = false;
        _grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grilla.SelectionChanged += (_, _) => CargarSeleccionEnFormulario();

        Controls.AddRange([
            etiquetaCodigo, _campoCodigo,
            etiquetaTitulo, _campoTitulo,
            etiquetaIsbn, _campoIsbn,
            etiquetaCantidad, _campoCantidad,
            etiquetaAutor, _comboAutor,
            etiquetaCategoria, _comboCategoria,
            _botonRegistrar, _botonActualizar, _botonEliminar, _botonLimpiar,
            etiquetaBuscar, _campoBuscarTitulo, _botonBuscar,
            _grilla
        ]);
    }

    private async Task CargarAutoresYCategoriasAsync()
    {
        var autores = await _servicioAutor.ObtenerTodosAsync();
        _autoresPorId.Clear();
        foreach (var autor in autores)
            _autoresPorId[autor.IdAutor] = autor;

        _comboAutor.Items.Clear();
        _comboAutor.Items.AddRange(autores.Select(a => new OpcionCombo(a.IdAutor, a.NombreCompleto)).ToArray());

        var categorias = await _servicioCategoria.ObtenerTodasAsync();
        _categoriasPorId.Clear();
        foreach (var categoria in categorias)
            _categoriasPorId[categoria.IdCategoria] = categoria;

        _comboCategoria.Items.Clear();
        _comboCategoria.Items.AddRange(categorias.Select(c => new OpcionCombo(c.IdCategoria, c.Nombre)).ToArray());
    }

    private async Task CargarLibrosAsync(IReadOnlyList<Libro>? librosPrecargados = null)
    {
        var libros = librosPrecargados ?? await _servicioLibro.ObtenerTodosAsync();

        _librosPorId.Clear();
        foreach (var libro in libros)
            _librosPorId[libro.IdLibro] = libro;

        _grilla.DataSource = libros
            .Select(libro => new LibroFila(
                libro.IdLibro, libro.Codigo, libro.Titulo, libro.Isbn,
                libro.Cantidad, libro.Disponible, libro.Autor.NombreCompleto, libro.Categoria.Nombre))
            .ToList();

        if (_grilla.Columns["IdLibro"] is DataGridViewColumn columnaId)
            columnaId.Visible = false;
    }

    private async Task BuscarAsync()
    {
        if (string.IsNullOrWhiteSpace(_campoBuscarTitulo.Text))
        {
            await CargarLibrosAsync();
            return;
        }

        var resultados = await _servicioLibro.BuscarPorTituloAsync(_campoBuscarTitulo.Text);
        await CargarLibrosAsync(resultados);
    }

    private void CargarSeleccionEnFormulario()
    {
        if (_grilla.CurrentRow?.DataBoundItem is not LibroFila fila || !_librosPorId.TryGetValue(fila.IdLibro, out var libro))
            return;

        _idLibroSeleccionado = libro.IdLibro;
        _campoCodigo.Text = libro.Codigo;
        _campoTitulo.Text = libro.Titulo;
        _campoIsbn.Text = libro.Isbn;
        _campoCantidad.Value = libro.Cantidad;
        _comboAutor.SelectedItem = _comboAutor.Items.Cast<OpcionCombo>().FirstOrDefault(o => o.Id == libro.Autor.IdAutor);
        _comboCategoria.SelectedItem = _comboCategoria.Items.Cast<OpcionCombo>().FirstOrDefault(o => o.Id == libro.Categoria.IdCategoria);
    }

    private async Task RegistrarAsync()
    {
        if (!TryObtenerAutorYCategoriaSeleccionados(out var autor, out var categoria))
            return;

        try
        {
            var libro = new Libro(_campoCodigo.Text, _campoTitulo.Text, _campoIsbn.Text, (int)_campoCantidad.Value, autor, categoria);
            await _servicioLibro.RegistrarAsync(libro);
            MessageBox.Show(this, "Libro registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarLibrosAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ActualizarAsync()
    {
        if (_idLibroSeleccionado is not int idLibro)
        {
            MessageBox.Show(this, "Selecciona un libro de la lista para actualizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryObtenerAutorYCategoriaSeleccionados(out var autor, out var categoria))
            return;

        try
        {
            var disponibleActual = _librosPorId[idLibro].Disponible;
            var nuevaCantidad = (int)_campoCantidad.Value;
            var nuevoDisponible = Math.Min(disponibleActual, nuevaCantidad);

            var libro = new Libro(idLibro, _campoCodigo.Text, _campoTitulo.Text, _campoIsbn.Text, nuevaCantidad, nuevoDisponible, autor, categoria);
            await _servicioLibro.ActualizarAsync(libro);
            MessageBox.Show(this, "Libro actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarLibrosAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task EliminarAsync()
    {
        if (_idLibroSeleccionado is not int idLibro)
        {
            MessageBox.Show(this, "Selecciona un libro de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(this, "¿Seguro que deseas eliminar este libro?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmacion != DialogResult.Yes)
            return;

        try
        {
            await _servicioLibro.EliminarAsync(idLibro);
            MessageBox.Show(this, "Libro eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarLibrosAsync();
        }
        catch (ExcepcionNegocio ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private bool TryObtenerAutorYCategoriaSeleccionados(out Autor autor, out Categoria categoria)
    {
        autor = null!;
        categoria = null!;

        if (_comboAutor.SelectedItem is not OpcionCombo opcionAutor || !_autoresPorId.TryGetValue(opcionAutor.Id, out var autorSeleccionado))
        {
            MessageBox.Show(this, "Selecciona un autor.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (_comboCategoria.SelectedItem is not OpcionCombo opcionCategoria || !_categoriasPorId.TryGetValue(opcionCategoria.Id, out var categoriaSeleccionada))
        {
            MessageBox.Show(this, "Selecciona una categoría.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        autor = autorSeleccionado;
        categoria = categoriaSeleccionada;
        return true;
    }

    private void LimpiarFormulario()
    {
        _idLibroSeleccionado = null;
        _campoCodigo.Clear();
        _campoTitulo.Clear();
        _campoIsbn.Clear();
        _campoCantidad.Value = 1;
        _comboAutor.SelectedItem = null;
        _comboCategoria.SelectedItem = null;
        _campoBuscarTitulo.Clear();
    }
}
