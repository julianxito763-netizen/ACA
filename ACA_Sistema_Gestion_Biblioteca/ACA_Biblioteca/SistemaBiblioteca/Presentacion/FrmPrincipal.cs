using SistemaBiblioteca.Negocio;

namespace SistemaBiblioteca.Presentacion;

public class FrmPrincipal : Form
{
    public FrmPrincipal()
    {
        Text = "Sistema de Gestión de Biblioteca"; StartPosition = FormStartPosition.CenterScreen;
        Width = 1050; Height = 680; MinimumSize = new Size(850, 550); BackColor = Color.FromArgb(245, 247, 250);
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 115, BackColor = Color.FromArgb(31, 55, 78) };
        encabezado.Controls.Add(new Label { Text = "SISTEMA DE GESTIÓN DE BIBLIOTECA", ForeColor = Color.White, Font = new Font("Segoe UI", 21, FontStyle.Bold), AutoSize = true, Location = new Point(28, 25) });
        encabezado.Controls.Add(new Label { Text = "Administración de libros, usuarios, préstamos y devoluciones", ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(32, 68) });
        Controls.Add(encabezado);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(35), AutoScroll = true, BackColor = BackColor };
        Controls.Add(panel);
        encabezado.BringToFront();
        Agregar("Libros", () => new FrmModulo(ConfiguracionModulo.Libros));
        Agregar("Autores", () => new FrmModulo(ConfiguracionModulo.Autores));
        Agregar("Editoriales", () => new FrmModulo(ConfiguracionModulo.Editoriales));
        Agregar("Usuarios", () => new FrmModulo(ConfiguracionModulo.Usuarios));
        Agregar("Préstamos y devoluciones", () => new FrmPrestamos());
        Agregar("Reportes", () => new FrmReportes());
        Agregar("Acerca del sistema", () => new FrmAcerca());
        void Agregar(string texto, Func<Form> abrir)
        {
            var boton = new Button { Text = texto, Width = 275, Height = 105, Margin = new Padding(12), BackColor = Color.White, ForeColor = Color.FromArgb(31,55,78), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            boton.FlatAppearance.BorderColor = Color.LightGray; boton.Click += (_, _) => abrir().ShowDialog(this); panel.Controls.Add(boton);
        }
    }
}

public class FrmAcerca : Form
{
    public FrmAcerca()
    {
        Text = "Acerca del sistema"; Width = 480; Height = 260; StartPosition = FormStartPosition.CenterParent;
        Controls.Add(new Label { Dock = DockStyle.Fill, Padding = new Padding(25), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11), Text = "Sistema de Gestión de Biblioteca\n\nProyecto académico – Programación Avanzada\nDesarrollado en C# Windows Forms y SQL Server." });
    }
}
