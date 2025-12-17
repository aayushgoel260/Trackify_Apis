using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly TrackifyContext _trackifyContext;
        public ProjectsController(TrackifyContext trackifyContext)
        {
            _trackifyContext = trackifyContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _trackifyContext.Projects.ToListAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProjectById(int id)
        {
            var project = await _trackifyContext.Projects
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.IsActive
                })
                .FirstOrDefaultAsync();
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _trackifyContext.Projects.FindAsync(id);
            if(project == null)
            {
                return NotFound();
            }
            _trackifyContext.Projects.Remove(project);
            await _trackifyContext.SaveChangesAsync();
            return Ok(project);
        }
    }
}
