namespace Project.Api.Middleware
{
    internal sealed class CorrelationIdMiddleware : IMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString("N");
                context.Request.Headers[HeaderName] = cid!;
            }

            context.Response.Headers[HeaderName] = cid!;
            context.Items[HeaderName] = cid!.ToString();

            await next(context).ConfigureAwait(false);
        }
    }
}