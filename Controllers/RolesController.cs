using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly TrackifyContext _trackifyContext;
        public RolesController(TrackifyContext trackifyContext)
        {
            _trackifyContext = trackifyContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var roles = await _trackifyContext.Roles.ToListAsync();
            return Ok(roles);
        }
    }
}
