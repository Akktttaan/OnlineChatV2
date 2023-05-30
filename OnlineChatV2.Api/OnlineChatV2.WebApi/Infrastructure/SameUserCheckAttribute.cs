using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Infrastructure;

/// <summary>
/// Проверяем что отправитель отправил свой id в параметрах API метода
/// </summary>
public class SameUserCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.ContainsKey("userId"))
        {
            if (context.ActionArguments["userId"] is not long userId)
            {
                context.Result = new BadRequestResult();
                return;
            }
            
            var user = (User)context.HttpContext.Items["Users"];

            if (user == null)
            {
                // Если user не приложен к контексту, возвращаем ошибку 401 Unauthorized
                context.Result = new StatusCodeResult(401);
            }
            
            var userIdFromContext = user.Id;

            if (userId != userIdFromContext)
            {
                // Если Id пользователя не совпадает, возвращаем ошибку 403 Forbidden
                context.Result = new StatusCodeResult(403);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}