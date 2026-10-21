using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ClinicConnectApi.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "Please try again later.")
            };

            if(statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext{
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                }
            });
        }
    }
}