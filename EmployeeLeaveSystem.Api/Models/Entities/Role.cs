using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveSystem.Api.Models.Entities;

public class Role
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
