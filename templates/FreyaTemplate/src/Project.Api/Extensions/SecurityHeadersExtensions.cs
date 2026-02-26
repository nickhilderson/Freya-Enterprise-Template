namespace Project.Api.Extensions;

/// <summary>
///     Adds a hardened set of HTTP security headers for enterprise-grade APIs.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    ///     Adds enterprise security headers (CSP, clickjacking protection, HSTS in production, etc.).
    ///     Use this early in the pipeline (before endpoints).
    /// </summary>
    public static IApplicationBuilder UseFreyaEnterpriseSecurityHeaders(
        this IApplicationBuilder app,
        IHostEnvironment env,
        Action<FreyaSecurityHeadersOptions>? configure = null)
    {
        var options = new FreyaSecurityHeadersOptions();
        configure?.Invoke(options);

        // HSTS must be applied only when HTTPS is guaranteed (usually production)
        if (env.IsProduction() && options.EnableHsts) app.UseHsts();

        app.Use(async (context, next) =>
        {
            // Skip if the response already started
            if (!context.Response.HasStarted) ApplySecurityHeaders(context, options);

            await next().ConfigureAwait(false);
        });

        return app;
    }

    private static void ApplySecurityHeaders(HttpContext context, FreyaSecurityHeadersOptions options)
    {
        // --- Clickjacking protection ---
        // Modern (preferred)
        TryAdd(context, "Content-Security-Policy", options.ContentSecurityPolicy);

        // Legacy fallback for older user agents
        TryAdd(context, "X-Frame-Options", options.XFrameOptions);

        // --- MIME sniffing protection ---
        TryAdd(context, "X-Content-Type-Options", "nosniff");

        // --- Referrer leakage reduction ---
        TryAdd(context, "Referrer-Policy", options.ReferrerPolicy);

        // --- Permissions Policy (formerly Feature Policy) ---
        TryAdd(context, "Permissions-Policy", options.PermissionsPolicy);

        // --- Cross-origin isolation safety defaults (careful: don't enable Cross Origin Opener Policy unless you know you need it) ---
        if (!string.IsNullOrWhiteSpace(options.CrossOriginOpenerPolicy))
            TryAdd(context, "Cross-Origin-Opener-Policy", options.CrossOriginOpenerPolicy);

        if (!string.IsNullOrWhiteSpace(options.CrossOriginResourcePolicy))
            TryAdd(context, "Cross-Origin-Resource-Policy", options.CrossOriginResourcePolicy);

        // X-XSS-Protection is obsolete; modern browsers ignore it. Keep it off to avoid a false sense of security.
        // TryAdd(context, "X-XSS-Protection", "0");
    }

    private static void TryAdd(HttpContext context, string name, string value)
    {
        if (!context.Response.Headers.ContainsKey(name)) context.Response.Headers.TryAdd(name, value);
    }
}

/// <summary>
///     Options for configuring Freya enterprise security headers.
/// </summary>
public sealed class FreyaSecurityHeadersOptions
{
    /// <summary>
    ///     Enables HTTP Strict Transport Security (HSTS). Only applied in production.
    ///     Ensure HTTPS is enforced at the edge/reverse proxy before enabling.
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    ///     CSP header value. For APIs, the safest default is to allow nothing and only set frame ancestors.
    ///     If you host Swagger UI, you may need to relax this for swagger routes.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } =
        "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

    /// <summary>
    ///     Legacy clickjacking header. DENY is strongest.
    /// </summary>
    public string XFrameOptions { get; set; } = "DENY";

    /// <summary>
    ///     Referrer-Policy header value.
    /// </summary>
    public string ReferrerPolicy { get; set; } = "no-referrer";

    /// <summary>
    ///     Permissions-Policy header value. API-safe default disables sensitive features.
    /// </summary>
    public string PermissionsPolicy { get; set; } =
        "camera=(), microphone=(), geolocation=(), payment=(), usb=(), interest-cohort=()";

    /// <summary>
    ///     COOP header. A good default is same-origin for apps with UI; for pure APIs this is optional.
    ///     If you serve Swagger UI, this can stay.
    /// </summary>
    public string CrossOriginOpenerPolicy { get; set; } = "same-origin";

    /// <summary>
    ///     CORP header. Helps prevent other sites from loading your resources cross-origin.
    ///     For APIs, 'same-site' is a safe middle ground.
    /// </summary>
    public string CrossOriginResourcePolicy { get; set; } = "same-site";
}