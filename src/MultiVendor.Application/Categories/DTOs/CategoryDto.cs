namespace MultiVendor.Application.Categories.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
}
