namespace Backend.DTO.Task;

/// <summary>
/// Parámetros utilizados para controlar la paginación de resultados.
/// </summary>
public sealed class PaginationParams
{
    // Número de página solicitado.
    // Las páginas comienzan en 1.
    public int Page { get; init; } = 1;

    // Cantidad de elementos por página.
    public int PageSize { get; init; } = 10;

    // Límite máximo de elementos que se pueden solicitar.
    // Evita peticiones excesivamente grandes.
    public const int MaxPageSize = 50;

    /// <summary>
    /// Cantidad de elementos que deben omitirse en la consulta SQL.
    /// </summary>
    public int Skip =>
        (Page - 1) * PageSize;
}