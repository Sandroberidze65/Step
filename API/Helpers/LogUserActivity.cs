using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;
public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContent = await next();

        if(!resultContent.HttpContext.User.Identity.IsAuthenticated) return;

        var userId = resultContent.HttpContext.User.GetUserId();

        var uow = resultContent.HttpContext.RequestServices.GetService<IUnitOfWork>();
        var user = await uow.UserRepository.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;
        await uow.Complete();
    }
}
