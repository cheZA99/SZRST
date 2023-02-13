using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SZRST.API.Context;
using SZRST.API.Helpers;
using SZRST.API.Models;

namespace SZRST.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikController : ControllerBase
    {
        private readonly SZRSTContext dbContext;

        public KorisnikController(SZRSTContext DbContext)
        {
            dbContext = DbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Korisnik userObj)
        {
            if (userObj == null)
            {
                return BadRequest();
            }

            var user = await dbContext.Korisnik.FirstOrDefaultAsync(x => x.KorisnickoIme == userObj.KorisnickoIme && x.Lozinka == userObj.Lozinka);
            if (user == null)
                return NotFound(new{ Message = "Korisnik ne postoji!" });
            
            return Ok(new
            {
                Message = "Prijava uspješna!"
            }); 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register ([FromBody] Korisnik userObj)
        {
            if (userObj == null)
                return BadRequest();

            if (await CheckUserNameExistAsync(userObj.KorisnickoIme))
                return BadRequest(new { Message = "Korisničko Ime već postoji!" });

            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email već postoji!" });

            var passwordCheck = CheckPasswordStrength(userObj.Lozinka);
            if (!string.IsNullOrEmpty(passwordCheck))
                return BadRequest(new { Message = passwordCheck.ToString() });

            userObj.Lozinka = PasswordHash.HashPassword(userObj.Lozinka);
            userObj.Rola = "Admin";
            userObj.Token = "123";
            await dbContext.AddAsync(userObj);
            await dbContext.SaveChangesAsync();
            return Ok( new
            {
                Message = "Korisnik uspješno registrovan!"
            });

        }

        private Task<bool> CheckUserNameExistAsync(string korisnickoIme)
        {
             return dbContext.Korisnik.AnyAsync(x => x.KorisnickoIme == korisnickoIme);
        }

        private Task<bool> CheckEmailExistAsync(string email)
        {
            return dbContext.Korisnik.AnyAsync(x => x.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            string specialCharRegex = "[!,#,$,<,>,%,&,/,(,),=,?,*.+,-,_]";
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Lozinka treba da sadrži minimalno 8 znakova!" + Environment.NewLine);
            if(!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Lozinka treba da sadrži alfanumeričke znakove!" + Environment.NewLine);
            if (!Regex.IsMatch(password, specialCharRegex))
                sb.Append("Lozinka treba da sadrži sepcijalni karakter!" + Environment.NewLine);

            return sb.ToString();
        }
    }
}
