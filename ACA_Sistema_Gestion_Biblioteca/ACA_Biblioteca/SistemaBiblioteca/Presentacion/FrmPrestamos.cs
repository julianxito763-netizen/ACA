using SistemaBiblioteca.Negocio;
using SistemaBiblioteca.Datos;

namespace SistemaBiblioteca.Presentacion;

public class FrmPrestamos : Form
{
    private readonly DataGridView _grid = new();
    private readonly ComboBox _usuario = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _libro = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _fecha = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _limite = new() { Format = DateTimePickerFormat.Short };
    public FrmPrestamos()
    {
        Text = "Préstamos y devoluciones"; Width = 1050; Height = 650; StartPosition = FormStartPosition.CenterParent;
        var superior = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(31,55,78) }; superior.Controls.Add(new Label { Text = "Préstamos y devoluciones", ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(20, 12) }); Controls.Add(superior);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 180, Padding = new Padding(12), WrapContents = true }; Controls.Add(panel);
        AgregarCampo("Usuario", _usuario); AgregarCampo("Libro", _libro); AgregarCampo("Fecha préstamo", _fecha); AgregarCampo("Fecha límite", _limite);
        var crear = new Button { Text = "Registrar préstamo", Width = 155, Height = 35, Margin = new Padding(8, 25, 8, 0) }; crear.Click += (_, _) => Crear(); panel.Controls.Add(crear);
        var devolver = new Button { Text = "Registrar devolución", Width = 165, Height = 35, Margin = new Padding(8, 25, 8, 0) }; devolver.Click += (_, _) => Devolver(); panel.Controls.Add(devolver);
        var editar = new Button { Text = "Actualizar fechas", Width = 135, Height = 35, Margin = new Padding(8, 25, 8, 0) }; editar.Click += (_, _) => Editar(); panel.Controls.Add(editar);
        var eliminar = new Button { Text = "Eliminar préstamo", Width = 145, Height = 35, Margin = new Padding(8, 25, 8, 0) }; eliminar.Click += (_, _) => Eliminar(); panel.Controls.Add(eliminar);
        var recargar = new Button { Text = "Actualizar", Width = 110, Height = 35, Margin = new Padding(8, 25, 8, 0) }; recargar.Click += (_, _) => Cargar(); panel.Controls.Add(recargar);
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(_grid);
        panel.BringToFront(); superior.BringToFront(); CargarOpciones(); Cargar();
        void AgregarCampo(string titulo, Control control) { var g = new Panel { Width = control.Width + 15, Height = 70, Margin = new Padding(5) }; g.Controls.Add(new Label { Text = titulo, AutoSize = true }); control.Location = new Point(0, 25); g.Controls.Add(control); panel.Controls.Add(g); }
    }
    private void CargarOpciones()
    {
        try
        {
            _usuario.DataSource = Db.Consultar("SELECT UsuarioId, Documento + ' - ' + Nombre + ' ' + Apellido AS Descripcion FROM Usuarios ORDER BY Apellido,Nombre");
            _usuario.DisplayMember = "Descripcion"; _usuario.ValueMember = "UsuarioId"; _usuario.SelectedIndex = -1;
            _libro.DataSource = Db.Consultar("SELECT LibroId, Codigo + ' - ' + Titulo AS Descripcion FROM Libros WHERE Existencias > 0 ORDER BY Titulo");
            _libro.DisplayMember = "Descripcion"; _libro.ValueMember = "LibroId"; _libro.SelectedIndex = -1;
        }
        catch (Exception ex) { Error(ex); }
    }
    private void Cargar() { try { _grid.DataSource = ServicioBiblioteca.ListarPrestamos(); } catch (Exception ex) { Error(ex); } }
    private void Crear()
    {
        try
        {
            if (_usuario.SelectedValue == null || _libro.SelectedValue == null) throw new ArgumentException("Seleccione un usuario y un libro.");
            int u = Convert.ToInt32(_usuario.SelectedValue); int l = Convert.ToInt32(_libro.SelectedValue);
            ServicioBiblioteca.CrearPrestamo(u, l, _fecha.Value, _limite.Value);
            MessageBox.Show("Préstamo registrado. Las existencias se actualizaron."); Cargar(); CargarOpciones();
        }
        catch (Exception ex) { Error(ex); }
    }
    private int PrestamoSeleccionado()
    {
        if (_grid.SelectedRows.Count == 0) throw new InvalidOperationException("Seleccione un préstamo en la tabla.");
        return Convert.ToInt32(_grid.SelectedRows[0].Cells["PrestamoId"].Value);
    }
    private void Devolver()
    {
        try { ServicioBiblioteca.RegistrarDevolucion(PrestamoSeleccionado()); MessageBox.Show("Devolución registrada. Las existencias se actualizaron."); Cargar(); CargarOpciones(); }
        catch (Exception ex) { Error(ex); }
    }
    private void Editar()
    {
        try { ServicioBiblioteca.EditarPrestamo(PrestamoSeleccionado(), _fecha.Value, _limite.Value); MessageBox.Show("Fechas actualizadas."); Cargar(); }
        catch (Exception ex) { Error(ex); }
    }
    private void Eliminar()
    {
        try
        {
            int id = PrestamoSeleccionado();
            if (MessageBox.Show("¿Eliminar el préstamo seleccionado? Si está activo, el libro volverá a las existencias.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            ServicioBiblioteca.EliminarPrestamo(id); MessageBox.Show("Préstamo eliminado."); Cargar(); CargarOpciones();
        }
        catch (Exception ex) { Error(ex); }
    }
    private static void Error(Exception ex) => MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
