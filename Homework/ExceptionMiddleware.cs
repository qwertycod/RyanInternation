using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Homework
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(new
                {
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                }.ToString());


            }


        }    
    }
}
