using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionTypesController:ControllerBase
    {
        private readonly TrackifyContext _trackifyContext;
        public ActionTypesController(TrackifyContext trackifyContext)
        {
            _trackifyContext = trackifyContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionType>>> GetActionTypes()
        {
            var actions=await _trackifyContext.ActionTypes.ToListAsync();
            return Ok(actions);
        }
    }
}
