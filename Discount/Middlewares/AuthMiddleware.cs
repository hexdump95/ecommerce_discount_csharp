using System.Net;

using Discount.Middlewares.Models;
using Discount.Token;

namespace Discount.Middlewares
{
    public class AuthMiddleware

    {
        private readonly ILogger<AuthMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public AuthMiddleware(ILogger<AuthMiddleware> logger, RequestDelegate next, IConfiguration configuration
        )
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, ITokenService tokenService, LoggedInUser loggedInUser)
        {
            var bearerToken = context.Request.Headers.Authorization.FirstOrDefault();

            if (bearerToken == null || !bearerToken.StartsWith("bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var token = bearerToken.Split(" ")[1];
            var userInDb = await tokenService.FindByToken(token);
            if (userInDb != null)
            {
                context.Items.Add("loggedInUser", userInDb);
                
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

                context.Items.Add("loggedInUser", user);

                await tokenService.SaveToken(token, user);

                _logger.LogDebug("token from auth service");
            }

            await _next(context);
        }
    }
}
