namespace Backend.Configuration;

    /// <summary>
    /// Configuración utilizada para generar y validar tokens JWT.
    /// Los valores reales se obtienen desde la configuración de la aplicación.
    /// </summary>
    public sealed class JwtSettings
    {
    // Clave secreta utilizada para firmar los tokens.
    public string Secret { get; init; } = string.Empty;

    // Emisor esperado del token.
    public string Issuer { get; init; } = string.Empty;

    // Audiencia esperada del token.
    public string Audience { get; init; } = string.Empty;

    // Tiempo de vida del access token en minutos.
    public int ExpirationMinutes { get; init; }
}