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

        var repo = resultContent.HttpContext.RequestServices.GetService<IUserRepository>();
        var user = await repo.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
