using System.Net.Mime;

using Discount.Middlewares.Models;
using Discount.Rest.Dtos;

using Microsoft.AspNetCore.Mvc;

namespace Discount.Rest;

[ApiController]
public class PostCoupon : ControllerBase
{
    private readonly User _user;
    private readonly ILogger<PostCoupon> _logger;

    public PostCoupon(User user, ILogger<PostCoupon> logger)
    {
        _user = user;
        _logger = logger;
    }

    [HttpPost("v1/coupons")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateCouponRequest>> Post([FromBody] CreateCouponRequest request)
    {
        await Task.CompletedTask;
        return Ok(request);
    }
}
