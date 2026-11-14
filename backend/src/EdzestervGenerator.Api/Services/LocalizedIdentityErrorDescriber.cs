using Microsoft.AspNetCore.Identity;

namespace EdzestervGenerator.Api.Services;

public sealed class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private static bool En => LocalizationText.IsEnglish;
    private static IdentityError Error(string code, string hu, string en) => new() { Code = code, Description = En ? en : hu };
    public override IdentityError DefaultError() => Error(nameof(DefaultError), "Ismeretlen hiba történt.", "An unknown error occurred.");
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), $"A(z) {email} email-cím már használatban van.", $"Email '{email}' is already taken.");
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), $"A(z) {userName} felhasználónév már használatban van.", $"Username '{userName}' is already taken.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Érvénytelen email-cím.", "Invalid email address.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"A jelszónak legalább {length} karakter hosszúnak kell lennie.", $"Passwords must be at least {length} characters.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "A jelszónak legalább egy számjegyet kell tartalmaznia.", "Passwords must have at least one digit.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "A jelszónak legalább egy nagybetűt kell tartalmaznia.", "Passwords must have at least one uppercase letter.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "A jelszónak legalább egy kisbetűt kell tartalmaznia.", "Passwords must have at least one lowercase letter.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "A jelszónak legalább egy speciális karaktert kell tartalmaznia.", "Passwords must have at least one non-alphanumeric character.");
}
