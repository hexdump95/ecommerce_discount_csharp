namespace Discount.Rest.Dtos
{
    public class CreateCouponRequest
    {
        public string Code { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public float StartPrice { get; set; }
        public float DiscountLimit { get; set; }
        public string? UserId { get; set; }
    }
}
