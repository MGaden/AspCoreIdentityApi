using CoreIdentity.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, RequestDelegate next)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, IEmailService _emailService)
        {
            try
            {
                await _next(httpContext);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, $"Request {httpContext.Request?.Method}: {httpContext.Request?.Path.Value} failed");
                //await _emailService.SendSqlException(ex);
                await HandleSqlExceptionAsync(httpContext, ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Request {httpContext.Request?.Method}: {httpContext.Request?.Path.Value} failed");
                //await _emailService.SendException(ex);
                await HandleExceptionAsync(httpContext, ex).ConfigureAwait(false);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var result = JsonConvert.SerializeObject(new
            {
                Type = "General Exception",
                Exception = new
                {
                    Message = ex.Message,
                    Inner = ex.InnerException
                }
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(result);
        }

        private static Task HandleSqlExceptionAsync(HttpContext context, SqlException ex)
        {
            var errorList = new List<Object>();

            for (int i = 0; i < ex.Errors.Count; i++)
            {
                errorList.Add(new
                {
                    Message = ex.Errors[i].Message,
                    Procedure = ex.Errors[i].Procedure,
                    LineNumber = ex.Errors[i].LineNumber,
                    Source = ex.Errors[i].Source,
                    Server = ex.Errors[i].Server
                });
            }

            var result = JsonConvert.SerializeObject(new
            {
                Type = "SQL Exception",
                Exceptions = errorList
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(result);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
