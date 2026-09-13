using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.Presentacion;

public class FormPrestamos : Form
{
    private sealed record OpcionCombo(int Id, string Descripcion)
    {
        public override string ToString() => Descripcion;
    }

    private sealed record DetalleFila(int IdLibro, string Titulo, int Cantidad);

    private sealed record PrestamoFila(int IdPrestamo, string Usuario, DateTime FechaPrestamo, DateTime FechaDevolucionEsperada, string Estado, string Libros);

    private readonly IServicioPrestamo _servicioPrestamo;
    private readonly IServicioUsuario _servicioUsuario;
    private readonly IServicioLibro _servicioLibro;

    private readonly Dictionary<int, Libro> _librosPorId = [];
    private readonly Dictionary<int, int> _detallePendiente = [];
    private readonly Dictionary<int, Prestamo> _prestamosPorId = [];

    private readonly ComboBox _comboUsuario = new();
    private readonly ComboBox _comboLibro = new();
    private readonly NumericUpDown _campoCantidad = new();
    private readonly DateTimePicker _campoFechaDevolucion = new();
    private readonly Button _botonAgregarLibro = new();
    private readonly Button _botonQuitarLibro = new();
    private readonly DataGridView _grillaDetalle = new();
    private readonly Button _botonRegistrarPrestamo = new();

    private readonly DataGridView _grillaPrestamos = new();
    private readonly Button _botonRegistrarDevolucion = new();

    public FormPrestamos(IServicioPrestamo servicioPrestamo, IServicioUsuario servicioUsuario, IServicioLibro servicioLibro)
    {
        _servicioPrestamo = servicioPrestamo ?? throw new ArgumentNullException(nameof(servicioPrestamo));
        _servicioUsuario = servicioUsuario ?? throw new ArgumentNullException(nameof(servicioUsuario));
        _servicioLibro = servicioLibro ?? throw new ArgumentNullException(nameof(servicioLibro));

        InicializarComponentes();
        Load += async (_, _) =>
        {
            await CargarUsuariosYLibrosAsync();
            await CargarPrestamosAsync();
        };
    }

    private void InicializarComponentes()
    {
        Text = "Préstamos y Devoluciones";
        Width = 820;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;

        var etiquetaUsuario = new Label { Text = "Usuario:", Left = 20, Top = 20, Width = 80 };
        _comboUsuario.Left = 110; _comboUsuario.Top = 18; _comboUsuario.Width = 300;
        _comboUsuario.DropDownStyle = ComboBoxStyle.DropDownList;

        var etiquetaFecha = new Label { Text = "Devolución esperada:", Left = 430, Top = 20, Width = 140 };
        _campoFechaDevolucion.Left = 580; _campoFechaDevolucion.Top = 18; _campoFechaDevolucion.Width = 150;
        _campoFechaDevolucion.Format = DateTimePickerFormat.Short;
        _campoFechaDevolucion.Value = DateTime.Today.AddDays(7);

        var etiquetaLibro = new Label { Text = "Libro:", Left = 20, Top = 60, Width = 80 };
        _comboLibro.Left = 110; _comboLibro.Top = 58; _comboLibro.Width = 350;
        _comboLibro.DropDownStyle = ComboBoxStyle.DropDownList;

        var etiquetaCantidad = new Label { Text = "Cantidad:", Left = 470, Top = 60, Width = 70 };
        _campoCantidad.Left = 545; _campoCantidad.Top = 58; _campoCantidad.Width = 70;
        _campoCantidad.Minimum = 1;
        _campoCantidad.Maximum = 100;
        _campoCantidad.Value = 1;

        _botonAgregarLibro.Text = "Agregar";
        _botonAgregarLibro.Left = 630; _botonAgregarLibro.Top = 56; _botonAgregarLibro.Width = 80;
        _botonAgregarLibro.Click += (_, _) => AgregarLibroADetalle();

        _botonQuitarLibro.Text = "Quitar";
        _botonQuitarLibro.Left = 715; _botonQuitarLibro.Top = 56; _botonQuitarLibro.Width = 80;
        _botonQuitarLibro.Click += (_, _) => QuitarLibroDeDetalle();

        _grillaDetalle.Left = 20; _grillaDetalle.Top = 100; _grillaDetalle.Width = 775; _grillaDetalle.Height = 130;
        _grillaDetalle.ReadOnly = true;
        _grillaDetalle.AllowUserToAddRows = false;
        _grillaDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grillaDetalle.MultiSelect = false;
        _grillaDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _botonRegistrarPrestamo.Text = "Registrar préstamo";
        _botonRegistrarPrestamo.Left = 20; _botonRegistrarPrestamo.Top = 240; _botonRegistrarPrestamo.Width = 160;
        _botonRegistrarPrestamo.Click += async (_, _) => await RegistrarPrestamoAsync();

        var etiquetaHistorico = new Label { Text = "Préstamos registrados:", Left = 20, Top = 285, Width = 200 };

        _grillaPrestamos.Left = 20; _grillaPrestamos.Top = 310; _grillaPrestamos.Width = 775; _grillaPrestamos.Height = 280;
        _grillaPrestamos.ReadOnly = true;
        _grillaPrestamos.AllowUserToAddRows = false;
        _grillaPrestamos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grillaPrestamos.MultiSelect = false;
        _grillaPrestamos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _botonRegistrarDevolucion.Text = "Registrar devolución del préstamo seleccionado";
        _botonRegistrarDevolucion.Left = 20; _botonRegistrarDevolucion.Top = 600; _botonRegistrarDevolucion.Width = 300;
        _botonRegistrarDevolucion.Click += async (_, _) => await RegistrarDevolucionAsync();

        Controls.AddRange([
            etiquetaUsuario, _comboUsuario,
            etiquetaFecha, _campoFechaDevolucion,
            etiquetaLibro, _comboLibro,
            etiquetaCantidad, _campoCantidad,
            _botonAgregarLibro, _botonQuitarLibro,
            _grillaDetalle,
            _botonRegistrarPrestamo,
            etiquetaHistorico, _grillaPrestamos,
            _botonRegistrarDevolucion
        ]);
    }

    private async Task CargarUsuariosYLibrosAsync()
    {
        var usuarios = await _servicioUsuario.ObtenerTodosAsync();
        _comboUsuario.Items.Clear();
        _comboUsuario.Items.AddRange(usuarios
            .Select(usuario => new OpcionCombo(usuario.IdUsuario, $"{usuario.NombreCompleto} ({usuario.Documento})"))
            .ToArray());

        var libros = await _servicioLibro.ObtenerTodosAsync();
        _librosPorId.Clear();
        foreach (var libro in libros)
            _librosPorId[libro.IdLibro] = libro;

        _comboLibro.Items.Clear();
        _comboLibro.Items.AddRange(libros
            .Select(libro => new OpcionCombo(libro.IdLibro, $"{libro.Titulo} (Disponibles: {libro.Disponible})"))
            .ToArray());
    }

    private void AgregarLibroADetalle()
    {
        if (_comboLibro.SelectedItem is not OpcionCombo opcionLibro)
        {
            MessageBox.Show(this, "Selecciona un libro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cantidadSolicitada = (int)_campoCantidad.Value;
        var cantidadActualEnDetalle = _detallePendiente.GetValueOrDefault(opcionLibro.Id);
        _detallePendiente[opcionLibro.Id] = cantidadActualEnDetalle + cantidadSolicitada;

        ActualizarGrillaDetalle();
    }

    private void QuitarLibroDeDetalle()
    {
        if (_grillaDetalle.CurrentRow?.DataBoundItem is not DetalleFila fila)
        {
            MessageBox.Show(this, "Selecciona una línea del detalle para quitar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _detallePendiente.Remove(fila.IdLibro);
        ActualizarGrillaDetalle();
    }

    private void ActualizarGrillaDetalle()
    {
        _grillaDetalle.DataSource = _detallePendiente
            .Select(par => new DetalleFila(par.Key, _librosPorId[par.Key].Titulo, par.Value))
            .ToList();
    }

    private async Task RegistrarPrestamoAsync()
    {
        if (_comboUsuario.SelectedItem is not OpcionCombo opcionUsuario)
        {
            MessageBox.Show(this, "Selecciona un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_detallePendiente.Count == 0)
        {
            MessageBox.Show(this, "Agrega al menos un libro al préstamo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await _servicioPrestamo.RegistrarPrestamoAsync(opcionUsuario.Id, _detallePendiente, _campoFechaDevolucion.Value);
            MessageBox.Show(this, "Préstamo registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormularioPrestamo();
            await CargarUsuariosYLibrosAsync();
            await CargarPrestamosAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar el préstamo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task CargarPrestamosAsync()
    {
        var prestamos = await _servicioPrestamo.ObtenerTodosAsync();

        _prestamosPorId.Clear();
        foreach (var prestamo in prestamos)
            _prestamosPorId[prestamo.IdPrestamo] = prestamo;

        _grillaPrestamos.DataSource = prestamos
            .Select(prestamo => new PrestamoFila(
                prestamo.IdPrestamo,
                prestamo.Usuario.NombreCompleto,
                prestamo.FechaPrestamo,
                prestamo.FechaDevolucionEsperada,
                prestamo.Estado.ToString(),
                string.Join(", ", prestamo.Detalles.Select(detalle => $"{detalle.Libro.Titulo} x{detalle.Cantidad}"))))
            .ToList();

        if (_grillaPrestamos.Columns["IdPrestamo"] is DataGridViewColumn columnaId)
            columnaId.Visible = false;
    }

    private async Task RegistrarDevolucionAsync()
    {
        if (_grillaPrestamos.CurrentRow?.DataBoundItem is not PrestamoFila fila)
        {
            MessageBox.Show(this, "Selecciona un préstamo de la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(this, $"¿Confirmar la devolución del préstamo #{fila.IdPrestamo}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmacion != DialogResult.Yes)
            return;

        try
        {
            await _servicioPrestamo.RegistrarDevolucionAsync(fila.IdPrestamo);
            MessageBox.Show(this, "Devolución registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await CargarUsuariosYLibrosAsync();
            await CargarPrestamosAsync();
        }
        catch (ExcepcionNegocio ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar la devolución", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimpiarFormularioPrestamo()
    {
        _comboUsuario.SelectedItem = null;
        _comboLibro.SelectedItem = null;
        _campoCantidad.Value = 1;
        _campoFechaDevolucion.Value = DateTime.Today.AddDays(7);
        _detallePendiente.Clear();
        ActualizarGrillaDetalle();
    }
}
