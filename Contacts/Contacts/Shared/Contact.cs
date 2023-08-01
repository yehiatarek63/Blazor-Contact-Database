using System.ComponentModel.DataAnnotations;

namespace Contacts.Shared;

public class Contact
{
    public Guid Id { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public TitleState? Title { get; set; }
    [Required]
    public string? Description { get; set; }
    [Required]
    public DateTimeOffset DateOfBirth { get; set; }
    [Required]
    public bool? MarriageStatus { get; set; }
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public RoleState? Roles { get; set; }
}
public enum TitleState
{
    Mr,
    Mrs,
    Miss,
    Dr,
    Prof
}

public enum RoleState
{
    User,
    Admin
}