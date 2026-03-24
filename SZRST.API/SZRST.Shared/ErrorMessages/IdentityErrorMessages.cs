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
		  { "PasswordTooShort", "Lozinka mora biti dugačka najmanje {0} znakova." },
		  { "DuplicateEmail", "Email je već zauzet." },
		  { "DuplicateUserName", "Korisničko ime je već zauzeto." },
		  { "InvalidEmail", "Email nije u ispravnom formatu." },
		  { "InvalidUserName", "Korisničko ime nije ispravno." },
		  { "PasswordMismatch", "Lozinka nije ispravna." },
		  { "UserAlreadyHasPassword", "Korisnik već ima postavljenu lozinku." }
	   };
	}

	public string GetErrorMessage(string errorCode)
	{
		return _errorMessages.TryGetValue(errorCode, out var message) ? message : "Nepoznata greška.";
	}
}
