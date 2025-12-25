using System.ComponentModel.DataAnnotations;

namespace SZRST.Shared
{
	public class RegisterViewModel
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Username { get; set; }

		[Required]
		[StringLength(50)]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 5)]
		public string Password { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 5)]
		public string ConfirmPassword { get; set; }

		[Required]
		public int TenantId { get; set; }
	}
}