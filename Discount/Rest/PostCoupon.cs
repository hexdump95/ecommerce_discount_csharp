using System.Net.Mime;

using Discount.Rest.Dtos;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Discount.Rest;

[ApiController]
public class PostCoupon : ControllerBase
{
    public PostCoupon()
    {
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
