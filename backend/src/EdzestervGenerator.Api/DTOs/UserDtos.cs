using System.ComponentModel.DataAnnotations;

namespace EdzestervGenerator.Api.DTOs;

public record UserDto(int Id, string Name, string Email, DateTime CreatedAt);

// Used for both creating and updating a user.
public record SaveUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(200)] string Email);
