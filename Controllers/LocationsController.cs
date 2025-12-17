using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly TrackifyContext _trackifyContext;
        public LocationsController(TrackifyContext trackifyContext)
        {
            _trackifyContext = trackifyContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetAllLocationNames()
        {
            var locations = await _trackifyContext.Locations
                .ToListAsync();



            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocationById(int id)
        {
            var location = await _trackifyContext.Locations
                .Where(l => l.Id == id)
                .Select(l => new
                {
                    l.Id,
                    l.Name
                })
                .FirstOrDefaultAsync();

            if (location == null)
            {
                return NotFound();
            }
            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationDTO locationdto)
        {
            if (string.IsNullOrWhiteSpace(locationdto.Name))
            {
                return BadRequest("Location name is required");
            }

            var location = new Location
            {
                Name = locationdto.Name,
            };


            _trackifyContext.Locations.Add(location);
            await _trackifyContext.SaveChangesAsync();
            var createdLocation = new
            {
                location.Name
            };
            return Ok(createdLocation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, LocationDTO locationdto)
        {
            var existingLocation = await _trackifyContext.Locations.FindAsync(id);
            if (existingLocation == null)
            {
                return NotFound();
            }
            existingLocation.Name=locationdto.Name;

            await _trackifyContext.SaveChangesAsync();
            var updatedLocation = new
            {
                existingLocation.Id,
                existingLocation.Name
            };
            return Ok(updatedLocation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _trackifyContext.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            _trackifyContext.Locations.Remove(location);
            await _trackifyContext.SaveChangesAsync();
            return NoContent();
        }
    }
}