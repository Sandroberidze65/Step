using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;


namespace API.Extensions;
public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<AppUser>(opt=>{
            opt.Password.RequireNonAlphanumeric = false;
            
        }).AddRoles<AppRole>()
          .AddRoleManager<RoleManager<AppRole>>()
          .AddEntityFrameworkStores<DataContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                ValidateIssuer = false,
                ValidateAudience = false,
            };

            options.Events = new JwtBearerEvents{
                OnMessageReceived = context => {
                    var accsesToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if(!string.IsNullOrEmpty(accsesToken) && path.StartsWithSegments("/hubs")){
                        context.Token = accsesToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(opt => {
            opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
        });
        return services;
    }
}
