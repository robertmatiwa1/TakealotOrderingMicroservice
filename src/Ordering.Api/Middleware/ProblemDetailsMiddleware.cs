using Microsoft.AspNetCore.WebUtilities;

namespace Ordering.Api.Middleware
{
    public class ProblemDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        public ProblemDetailsMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try { await _next(context); }
            catch (ArgumentException aex) { await Write(context, 400, aex.Message); }
            catch (InvalidOperationException ioex) { await Write(context, 400, ioex.Message); }
            catch (Exception) { await Write(context, 500, "An unexpected error occurred."); }
        }

        private static Task Write(HttpContext ctx, int status, string detail)
        {
            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            var pd = new { type = "about:blank", title = ReasonPhrases.GetReasonPhrase(status), status, detail, traceId = ctx.TraceIdentifier };
            return ctx.Response.WriteAsJsonAsync(pd);
        }
    }

    public static class ProblemDetailsExtensions
    {
        public static IApplicationBuilder UseGlobalProblemDetails(this IApplicationBuilder app)
            => app.UseMiddleware<ProblemDetailsMiddleware>();
    }
}
