using System.ComponentModel.DataAnnotations;

namespace Nexora.Models
{
    public class Product
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string? Gender { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageUrls { get; set; }

        public string? Tags { get; set; }

        public string? Colors { get; set; }

        public List<string>? GetImages()
        {
            var images = new List<string>();
            if (!string.IsNullOrEmpty(ImageUrl))
                images.Add(ImageUrl);
            if (!string.IsNullOrEmpty(ImageUrls))
            {
                images.AddRange(ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(url => url.Trim()));
            }
            return images.Any() ? images : null;
        }

        public List<string>? GetTags()
        {
            return !string.IsNullOrEmpty(Tags)
                ? Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList()
                : null;
        }

        public List<string>? GetColors()
        {
            return !string.IsNullOrEmpty(Colors)
                ? Colors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(color => color.Trim())
                    .ToList()
                : null;
        }
    }
}