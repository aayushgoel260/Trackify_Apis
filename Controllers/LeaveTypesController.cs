using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypesController : ControllerBase
    {
        private readonly TrackifyContext _trackifyContext;
        public LeaveTypesController(TrackifyContext trackifyContext)
        {
            _trackifyContext = trackifyContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveType>>> GetLeaveTypes()
        {
            var leaves = await _trackifyContext.LeaveTypes
                .Select(lt => new
                {
                    lt.Id,
                    lt.LeaveCategory
                })
                .ToListAsync();
            return Ok(leaves);
        }
    }
}
