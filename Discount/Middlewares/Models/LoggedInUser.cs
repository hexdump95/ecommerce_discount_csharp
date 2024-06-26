namespace Discount.Middlewares.Models
{
    public class LoggedInUser
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Login { get; set; } = null!;
        public List<string> Permissions { get; set; } = [];
        
    }
}
