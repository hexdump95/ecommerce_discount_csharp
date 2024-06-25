using System.Net;

using Discount.Middlewares.Models;

namespace Discount.Middlewares
{
    public class AuthMiddleware

    {
        private readonly ILogger<AuthMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly User _user;

        public AuthMiddleware(ILogger<AuthMiddleware> logger, RequestDelegate next, User user)
        {
            _next = next;
            _logger = logger;
            _user = user;
        }

        public async Task Invoke(HttpContext context)
        {
            var bearerToken = context.Request.Headers.Authorization.FirstOrDefault();

            if (bearerToken == null || !bearerToken.StartsWith("bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // First, check in the db
            var tokenInDb = false; // something like: tokenRepository.FindByToken(bearerToken);
            if (tokenInDb) // if exists, get the User and map to _user
            {
                _user.Id = "123";
                _user.Name = "user";
                _user.Login = "user";
                _user.Permissions = ["user", "admin"];

                _logger.LogInformation("token from db");
                await _next(context);
            }
            else // look for it in auth service
            {
                var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };

                httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken);

                using HttpResponseMessage response = await httpClient.GetAsync("v1/users/current");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var user = await response.Content.ReadFromJsonAsync<User>();

                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                _user.Id = user.Id;
                _user.Login = user.Login;
                _user.Name = user.Name;
                _user.Permissions = user.Permissions;

                // now save it in the db...

                await _next(context);
            }
        }
    }
}
