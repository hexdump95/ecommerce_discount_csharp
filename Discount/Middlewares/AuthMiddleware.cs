using System.Net;

using Discount.Middlewares.Models;
using Discount.Token;

namespace Discount.Middlewares
{
    public class AuthMiddleware

    {
        private readonly ILogger<AuthMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly LoggedInUser _user;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthMiddleware(ILogger<AuthMiddleware> logger, RequestDelegate next, LoggedInUser user,
            IConfiguration configuration
            , ITokenService tokenService
        )
        {
            _next = next;
            _logger = logger;
            _user = user;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public async Task Invoke(HttpContext context)
        {
            var bearerToken = context.Request.Headers.Authorization.FirstOrDefault();

            if (bearerToken == null || !bearerToken.StartsWith("bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var token = bearerToken.Split(" ")[1];
            var userInDb = await _tokenService.FindByToken(token);
            if (userInDb != null)
            {
                _user.Id = userInDb.Id;
                _user.Name = userInDb.Name;
                _user.Login = userInDb.Login;
                _user.Permissions = userInDb.Permissions;

                _logger.LogDebug("token from db");
            }
            else
            {
                var httpClient = new HttpClient { BaseAddress = new Uri(_configuration["Uris:AuthServiceUrl"]!) };

                httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken);

                using HttpResponseMessage response = await httpClient.GetAsync("v1/users/current");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var user = await response.Content.ReadFromJsonAsync<LoggedInUser>();

                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                _user.Id = user.Id;
                _user.Login = user.Login;
                _user.Name = user.Name;
                _user.Permissions = user.Permissions;

                await _tokenService.SaveToken(token, user);

                _logger.LogDebug("token from auth service");
            }

            await _next(context);
        }
    }
}
