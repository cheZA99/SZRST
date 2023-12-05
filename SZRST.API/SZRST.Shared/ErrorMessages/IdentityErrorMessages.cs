using System.Collections.Generic;

public class IdentityErrorMessages
{
    private readonly Dictionary<string, string> _errorMessages;

    public IdentityErrorMessages()
    {
        _errorMessages = new Dictionary<string, string>
        {
            { "PasswordRequiresUpper", "Lozinka mora sadržavati velika slova." },
            { "PasswordRequiresDigit", "Lozinka mora sadržavati brojeve." },
            { "PasswordRequiresLower", "Lozinka mora sadržavati mala slova." },
            { "PasswordRequiresNonAlphanumeric", "Lozinka mora sadržavati barem jedan specijalni znak." },
            { "PasswordTooShort", "Lozinka mora biti dugačka najmanje {0} znakova." }
            // Dodajte ili mijenjajte poruke grešaka po potrebi
        };
    }

    public string GetErrorMessage(string errorCode)
    {
        return _errorMessages.TryGetValue(errorCode, out var message) ? message : "Nepoznata greška.";
    }
}
