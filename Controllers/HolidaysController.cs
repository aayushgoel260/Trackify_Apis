using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using TrackifyApis.Models;

namespace TrackifyApis.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController:ControllerBase
    {

        private readonly IConfiguration _configuration;
        public HolidaysController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [HttpGet]
        public async Task<IActionResult> GetAllHolidays()
        {
            var holidays = new List<object>();
            using var conn = GetConnection();
            using var cmd = new SqlCommand("GetAllHolidays", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                holidays.Add(new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Date = reader.GetDateTime(2)
                });
            }
            return Ok(holidays);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHolidayById(int id)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("GetHolidayById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return NotFound();
            await reader.ReadAsync();
            var holiday = new
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Date = reader.GetDateTime(2)
            };
            return Ok(holiday);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHoliday([FromBody] HolidayDTO holidaydto)
        {
            if (holidaydto == null)
            {
                return BadRequest("Request body is missing.");
            }
            if (string.IsNullOrWhiteSpace(holidaydto.Name))
            {
                return BadRequest("Holiday Name is required.");
            }
            if (holidaydto.Date==default)
            {
                return BadRequest("Holiday Date is required.");
            }
            if (holidaydto.LocationId<=0)
            {
                return BadRequest("Valid LocationId is required.");
            }
            using var conn = GetConnection();
            using var cmd = new SqlCommand("CreateHoliday", conn) 
            { 
                CommandType = CommandType.StoredProcedure 
            };
            cmd.Parameters.AddWithValue("@Name", holidaydto.Name);
            cmd.Parameters.AddWithValue("@Date", holidaydto.Date);
            cmd.Parameters.AddWithValue("@LocationId", holidaydto.LocationId);
            cmd.Parameters.AddWithValue("@IsOptional", holidaydto.IsOptional);
            await conn.OpenAsync();
            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return CreatedAtAction(nameof(GetHolidayById), new { id = newId }, new { Id = newId, holidaydto.Name, holidaydto.Date });


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHoliday(int id, [FromBody] HolidayDTO dto)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("UpdateHoliday", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Date", dto.Date);
            cmd.Parameters.AddWithValue("@LocationId", dto.LocationId);
            cmd.Parameters.AddWithValue("@IsOptional", dto.IsOptional);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { Id = id, dto.Name, dto.Date });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand("DeleteHoliday", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return NoContent();

        }
    }
}
