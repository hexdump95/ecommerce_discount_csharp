using Discount.Grpc;
using Discount.Middlewares;
using Discount.Middlewares.Models;
using Discount.Token;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<LoggedInUser>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString = builder.Configuration["Uris:RedisUrl"]!;
    return ConnectionMultiplexer.Connect(connectionString);
});
builder.Services.AddGrpc();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<AuthMiddleware>();

// Configure the HTTP request pipeline.
app.MapGrpcService<GrpcCouponService>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
