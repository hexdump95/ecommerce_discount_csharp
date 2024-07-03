using Grpc.Core;

namespace Discount.Grpc
{
    public class GrpcCouponService : CouponService.CouponServiceBase
    {
        public override Task<CouponResponse> GetDiscount(CouponRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CouponResponse { Message = request.CouponCode });
        }
    }
}
