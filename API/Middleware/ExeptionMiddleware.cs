using System.Net;
using System.Text.Json;
using API.Errors;


namespace API.Middleware;
public class ExeptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExeptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    public ExeptionMiddleware(RequestDelegate next, ILogger<ExeptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context){
        try{
            await _next(context);
        }
        catch(Exception ex){
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var responce = _env.IsDevelopment() 
            ? new ApiExeption(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) : 
            new ApiExeption(context.Response.StatusCode, ex.Message, "Internal Server Error");

            var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

            var json = JsonSerializer.Serialize(responce, options);

            await context.Response.WriteAsync(json);
        }
    }
}
