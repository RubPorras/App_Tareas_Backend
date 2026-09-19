namespace Backend.DTO.Task;

/// <summary>
/// Representa una respuesta paginada.
/// </summary>
/// <typeparam name="T">Tipo de elemento contenido en la página.</typeparam>
public sealed class PagedResult<T>
{
    // Elementos correspondientes a la página actual.
    public IReadOnlyList<T> Items { get; init; } = [];

    // Número de página actual.
    public int Page { get; init; }

    // Cantidad de elementos solicitados por página.
    public int PageSize { get; init; }

    // Cantidad total de registros que cumplen los criterios.
    public int TotalCount { get; init; }

    // Cantidad total de páginas disponibles.
    public int TotalPages =>
        (int)Math.Ceiling(
            TotalCount / (double)PageSize);
}