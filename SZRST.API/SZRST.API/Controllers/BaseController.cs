using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Application.Responses;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T, TDb, TSearch, TInsert, TUpdate> : ControllerBase where T : class where TSearch : class where TInsert : class where TUpdate : class where TDb : class
    {
        protected readonly IBaseService<T, TDb, TSearch, TInsert, TUpdate> _service;
        public BaseController(IBaseService<T, TDb, TSearch, TInsert, TUpdate> service)
        {
            _service = service;
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
        [HttpGet]
        public virtual async Task<IActionResult> GetAsync([FromQuery] TSearch search)
        {
            var entities = await _service.GetAsync(search);
            return Ok(new PagedResponse<IEnumerable<T>>(entities, entities.CurrentPage, entities.PageSize, entities.TotalCount));
        }
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetByIdAsync(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }
        [HttpPost]
        public virtual async Task<IActionResult> InsertAsync([FromBody] TInsert request)
        {
            return Ok(await _service.InsertAsync(request));
        }
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> UpdateAsync(int id, [FromBody] TUpdate request)
        {
            return Ok(await _service.UpdateAsync(id, request));
        }
    }
}
