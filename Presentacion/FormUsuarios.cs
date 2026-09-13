using Biblioteca.Dominio;
using Biblioteca.LogicaNegocio.Contratos;
using Biblioteca.LogicaNegocio.Excepciones;

namespace Biblioteca.Presentacion;

public class FormUsuarios : Form
{
    private sealed record UsuarioFila(int IdUsuario, string Documento, string Nombre, string Apellidos, string Telefono, string Correo, string ProgramaAcademico);

    private readonly IServicioUsuario _servicioUsuario;

    private readonly DataGridView _grilla = new();
    private readonly TextBox _campoDocumento = new();
    private readonly TextBox _campoNombre = new();
    private readonly TextBox _campoApellidos = new();
    private readonly TextBox _campoTelefono = new();
    private readonly TextBox _campoCorreo = new();
    private readonly TextBox _campoProgramaAcademico = new();
    private readonly Button _botonRegistrar = new();
    private readonly Button _botonActualizar = new();
    private readonly Button _botonEliminar = new();
    private readonly Button _botonLimpiar = new();

    private int? _idUsuarioSeleccionado;

    public FormUsuarios(IServicioUsuario servicioUsuario)
    {
        _servicioUsuario = servicioUsuario ?? throw new ArgumentNullException(nameof(servicioUsuario));
        InicializarComponentes();
        Load += async (_, _) => await CargarUsuariosAsync();
    }

    private void InicializarComponentes()
    {
        Text = "Gestión de Usuarios";
        Width = 780;
        Height = 540;
        StartPosition = FormStartPosition.CenterParent;

        var etiquetaDocumento = new Label { Text = "Documento:", Left = 20, Top = 20, Width = 90 };
        _campoDocumento.Left = 120; _campoDocumento.Top = 18; _campoDocumento.Width = 180;

        var etiquetaNombre = new Label { Text = "Nombre:", Left = 320, Top = 20, Width = 70 };
        _campoNombre.Left = 400; _campoNombre.Top = 18; _campoNombre.Width = 180;

        var etiquetaApellidos = new Label { Text = "Apellidos:", Left = 20, Top = 55, Width = 90 };
        _campoApellidos.Left = 120; _campoApellidos.Top = 53; _campoApellidos.Width = 180;

        var etiquetaTelefono = new Label { Text = "Teléfono:", Left = 320, Top = 55, Width = 70 };
        _campoTelefono.Left = 400; _campoTelefono.Top = 53; _campoTelefono.Width = 180;

        var etiquetaCorreo = new Label { Text = "Correo:", Left = 20, Top = 90, Width = 90 };
        _campoCorreo.Left = 120; _campoCorreo.Top = 88; _campoCorreo.Width = 280;

        var etiquetaPrograma = new Label { Text = "Programa académico:", Left = 20, Top = 125, Width = 140 };
        _campoProgramaAcademico.Left = 170; _campoProgramaAcademico.Top = 123; _campoProgramaAcademico.Width = 280;

        _botonRegistrar.Text = "Registrar";
        _botonRegistrar.Left = 20; _botonRegistrar.Top = 165; _botonRegistrar.Width = 110;
        _botonRegistrar.Click += async (_, _) => await RegistrarAsync();

        _botonActualizar.Text = "Actualizar";
        _botonActualizar.Left = 140; _botonActualizar.Top = 165; _botonActualizar.Width = 110;
        _botonActualizar.Click += async (_, _) => await ActualizarAsync();

        _botonEliminar.Text = "Eliminar";
        _botonEliminar.Left = 260; _botonEliminar.Top = 165; _botonEliminar.Width = 110;
        _botonEliminar.Click += async (_, _) => await EliminarAsync();

        _botonLimpiar.Text = "Limpiar";
        _botonLimpiar.Left = 380; _botonLimpiar.Top = 165; _botonLimpiar.Width = 110;
        _botonLimpiar.Click += (_, _) => LimpiarFormulario();

        _grilla.Left = 20; _grilla.Top = 210; _grilla.Width = 720; _grilla.Height = 280;
        _grilla.ReadOnly = true;
        _grilla.AllowUserToAddRows = false;
        _grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grilla.MultiSelect = false;
        _grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grilla.SelectionChanged += (_, _) => CargarSeleccionEnFormulario();

        Controls.AddRange([
            etiquetaDocumento, _campoDocumento,
            etiquetaNombre, _campoNombre,
            etiquetaApellidos, _campoApellidos,
            etiquetaTelefono, _campoTelefono,
            etiquetaCorreo, _campoCorreo,
            etiquetaPrograma, _campoProgramaAcademico,
            _botonRegistrar, _botonActualizar, _botonEliminar, _botonLimpiar,
            _grilla
        ]);
    }

    private async Task CargarUsuariosAsync()
    {
        var usuarios = await _servicioUsuario.ObtenerTodosAsync();
        _grilla.DataSource = usuarios
            .Select(usuario => new UsuarioFila(
                usuario.IdUsuario, usuario.Documento, usuario.Nombre, usuario.Apellidos,
                usuario.Telefono, usuario.Correo, usuario.ProgramaAcademico))
            .ToList();

        if (_grilla.Columns["IdUsuario"] is DataGridViewColumn columnaId)
            columnaId.Visible = false;
    }

    private void CargarSeleccionEnFormulario()
    {
        if (_grilla.CurrentRow?.DataBoundItem is not UsuarioFila fila)
            return;

        _idUsuarioSeleccionado = fila.IdUsuario;
        _campoDocumento.Text = fila.Documento;
        _campoNombre.Text = fila.Nombre;
        _campoApellidos.Text = fila.Apellidos;
        _campoTelefono.Text = fila.Telefono;
        _campoCorreo.Text = fila.Correo;
        _campoProgramaAcademico.Text = fila.ProgramaAcademico;
    }

    private async Task RegistrarAsync()
    {
        try
        {
            var usuario = new Usuario(
                _campoDocumento.Text, _campoNombre.Text, _campoApellidos.Text,
                _campoTelefono.Text, _campoCorreo.Text, _campoProgramaAcademico.Text);
            await _servicioUsuario.RegistrarAsync(usuario);
            MessageBox.Show(this, "Usuario registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarUsuariosAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ActualizarAsync()
    {
        if (_idUsuarioSeleccionado is not int idUsuario)
        {
            MessageBox.Show(this, "Selecciona un usuario de la lista para actualizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var usuario = new Usuario(
                idUsuario, _campoDocumento.Text, _campoNombre.Text, _campoApellidos.Text,
                _campoTelefono.Text, _campoCorreo.Text, _campoProgramaAcademico.Text);
            await _servicioUsuario.ActualizarAsync(usuario);
            MessageBox.Show(this, "Usuario actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarUsuariosAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or ExcepcionNegocio)
        {
            MessageBox.Show(this, ex.Message, "No se pudo actualizar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task EliminarAsync()
    {
        if (_idUsuarioSeleccionado is not int idUsuario)
        {
            MessageBox.Show(this, "Selecciona un usuario de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(this, "¿Seguro que deseas eliminar este usuario?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmacion != DialogResult.Yes)
            return;

        try
        {
            await _servicioUsuario.EliminarAsync(idUsuario);
            MessageBox.Show(this, "Usuario eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarUsuariosAsync();
        }
        catch (ExcepcionNegocio ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimpiarFormulario()
    {
        _idUsuarioSeleccionado = null;
        _campoDocumento.Clear();
        _campoNombre.Clear();
        _campoApellidos.Clear();
        _campoTelefono.Clear();
        _campoCorreo.Clear();
        _campoProgramaAcademico.Clear();
    }
}
