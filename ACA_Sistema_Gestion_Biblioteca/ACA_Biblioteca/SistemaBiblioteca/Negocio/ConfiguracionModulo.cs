namespace SistemaBiblioteca.Negocio;

public sealed class ConfiguracionModulo
{
    public string Titulo { get; init; } = "";
    public string Tabla { get; init; } = "";
    public string Clave { get; init; } = "";
    public string ColumnasConsulta { get; init; } = "";
    public string Orden { get; init; } = "";
    public (string Nombre, string Etiqueta, bool Numerico)[] Campos { get; init; } = [];

    public static ConfiguracionModulo Libros => new()
    {
        Titulo = "Gestión de libros", Tabla = "Libros", Clave = "LibroId",
        ColumnasConsulta = "l.LibroId, l.Codigo, l.Titulo, a.Nombre + ' ' + a.Apellido AS Autor, e.Nombre AS Editorial, l.AnioPublicacion, l.Existencias",
        Orden = "l.LibroId",
        Campos = [("Codigo", "Código / ISBN", false), ("Titulo", "Título", false), ("AutorId", "ID del autor", true), ("EditorialId", "ID de editorial", true), ("AnioPublicacion", "Año de publicación", true), ("Existencias", "Existencias", true)]
    };
    public static ConfiguracionModulo Autores => new()
    {
        Titulo = "Gestión de autores", Tabla = "Autores", Clave = "AutorId",
        ColumnasConsulta = "AutorId, Nombre, Apellido", Orden = "AutorId",
        Campos = [("Nombre", "Nombre", false), ("Apellido", "Apellido", false)]
    };
    public static ConfiguracionModulo Editoriales => new()
    {
        Titulo = "Gestión de editoriales", Tabla = "Editoriales", Clave = "EditorialId",
        ColumnasConsulta = "EditorialId, Nombre, Ciudad", Orden = "EditorialId",
        Campos = [("Nombre", "Nombre", false), ("Ciudad", "Ciudad", false)]
    };
    public static ConfiguracionModulo Usuarios => new()
    {
        Titulo = "Gestión de usuarios", Tabla = "Usuarios", Clave = "UsuarioId",
        ColumnasConsulta = "UsuarioId, Nombre, Apellido, Documento, Telefono, Correo", Orden = "UsuarioId",
        Campos = [("Nombre", "Nombre", false), ("Apellido", "Apellido", false), ("Documento", "Documento", false), ("Telefono", "Teléfono", false), ("Correo", "Correo electrónico", false)]
    };
}
