namespace Nexora.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}