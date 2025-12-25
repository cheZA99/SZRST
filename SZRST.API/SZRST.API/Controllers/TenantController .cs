using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = Roles.SuperAdmin)]
	[Route("api/[controller]")]
	[ApiController]
	public class TenantController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public TenantController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/tenant
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllTenants()
		{
			var tenants = await _context.Set<Tenant>()
			    .Select(t => new TenantDto
			    {
				    Id = t.Id,
				    Name = t.Name
			    })
			    .ToListAsync();

			return Ok(tenants);
		}

		// GET: api/tenant/{id}
		[Authorize]
		[HttpGet("{id}")]
		public async Task<ActionResult<TenantDto>> GetTenant(int id)
		{
			var tenant = await _context.Set<Tenant>()
			    .Where(t => t.Id == id)
			    .Select(t => new TenantDto
			    {
				    Id = t.Id,
				    Name = t.Name
			    })
			    .FirstOrDefaultAsync();

			if (tenant == null)
			{
				return NotFound();
			}

			return Ok(tenant);
		}
	}

	public class TenantDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}