using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Payments.Abstractions;
using Payments.Abstractions.Exceptions;

namespace Payments.Middleware;

public class ErrorMiddleware : IMiddleware
{
    private readonly IOptions<JsonOptions> _jsonOptions;
    private readonly ILogger<ErrorMiddleware> _logger;

    public ErrorMiddleware(IOptions<JsonOptions> jsonOptions, ILogger<ErrorMiddleware> logger)
    {
        _jsonOptions = jsonOptions;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ApiException e)
        {
            if (e is InternalServerErrorException)
            {
                _logger.LogError(e, "An internal server error occured");
            }
            
            await WriteErrorAsync(context, e);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unhandled exception occured");

            await WriteErrorAsync(context, new InternalServerErrorException());
        }
    }

    private async Task WriteErrorAsync(HttpContext context, ApiException e)
    {
        var error = new ApiError
        {
            Type = e.Type,
            Message = e.Message,
            ParamName = e.ParamName
        };

        context.Response.StatusCode = (int)e.StatusCode;
        await JsonSerializer.SerializeAsync(context.Response.Body, error, _jsonOptions.Value.JsonSerializerOptions);
    }
}