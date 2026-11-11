using SistemaBiblioteca.Datos;
using SistemaBiblioteca.Presentacion;

namespace SistemaBiblioteca;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Db.ProbarConexion();
            Application.Run(new FrmPrincipal());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No fue posible conectar con SQL Server. Verifique que SQL Server esté instalado, " +
                "que la instancia SQLEXPRESS esté iniciada y que haya ejecutado BaseDatos/Biblioteca.sql.\n\n" +
                "Detalle: " + ex.Message,
                "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
