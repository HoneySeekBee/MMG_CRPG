using Application.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Filters
{
    public class GameExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is GameErrorException gex)
            {
                context.Result = new JsonResult(new
                {
                    error = gex.ErrorCode,
                    message = gex.Message
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
