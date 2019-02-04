using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Neembly.GPIDServer.WebAPI.Exceptions;
using System;
using System.Net;


namespace Neembly.GPIDServer.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        #region Member Variables
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        #endregion 

        #region Constructor
        public CustomExceptionFilterAttribute(
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = loggerFactory.CreateLogger<CustomExceptionFilterAttribute>();
        }
        #endregion

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(
                    ((ValidationException)context.Exception).Failures);

                return;
            }

            var code = HttpStatusCode.InternalServerError;

            if (context.Exception is NotFoundException)
            {
                code = HttpStatusCode.NotFound;
            }
            else
            {
                var username = context.HttpContext.User.Identity.Name;
                _logger.LogError(context.Exception,
                    "Path: {0} User: {1}",
                    context.HttpContext.Request.Path,
                    username);
            }

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)code;

            dynamic result = new
            {
                error = new[] { context.Exception.Message }
            };

            if (!_hostingEnvironment.IsProduction())
            {
                result = new
                {
                    error = new[] { context.Exception.Message },
                    stackTrace = context.Exception.StackTrace
                };
            }
            context.Result = new JsonResult(result);
        }
    }
}
