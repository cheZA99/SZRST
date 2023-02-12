using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SZRST.API.Context;
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
            await dbContext.AddAsync(userObj);
            await dbContext.SaveChangesAsync();
            return Ok( new
            {
                Message = "Korisnik uspješno registrovan!"
            });

        }
    }
}
