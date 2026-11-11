namespace SistemaBiblioteca.Modelos;

public class Autor
{
    public int AutorId { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
}

public class Editorial
{
    public int EditorialId { get; set; }
    public string Nombre { get; set; } = "";
    public string? Ciudad { get; set; }
}

public class Libro
{
    public int LibroId { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public int AutorId { get; set; }
    public int EditorialId { get; set; }
    public int AnioPublicacion { get; set; }
    public int Existencias { get; set; }
}

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Documento { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
}

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int UsuarioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; } = "Prestado";
}
