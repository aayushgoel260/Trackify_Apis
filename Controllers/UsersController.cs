using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using TrackifyApis.Helper;
using TrackifyApis.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        public UsersController(IConfiguration configuration, TokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = new List<User>();
            using var conn = GetConnection();
            using var cmd = new SqlCommand("spGetAllUsers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["EmailId"].ToString(),
                    LocationId = Convert.ToInt32(reader["LocationId"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                    ActionTypeId = Convert.ToInt32(reader["ActionTypeId"]),
                    ActionDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["ActionDate"]))
                });
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid user ID provided.");
            }

            using var conn = GetConnection();
            using var cmd = new SqlCommand("spGetUserById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                return NotFound($"User with ID {id} does not exist.");
            }

            await reader.ReadAsync();
            var user = new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"]?.ToString(),
                Email = reader["EmailId"]?.ToString(),
                LocationId = Convert.ToInt32(reader["LocationId"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                ActionTypeId = Convert.ToInt32(reader["ActionTypeId"]),
                ActionDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["ActionDate"]))
            };

            return Ok(user);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateOrUpdateUser([FromBody] UserDto userdto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var allowedIds = new[] { 1, 2, 3 };

            if (!allowedIds.Contains(userdto.LocationId))
                return BadRequest("Invalid LocationId");

            using var conn = GetConnection();
            await conn.OpenAsync();
            int actionTypeId;
            if (userdto.Id.HasValue)
            {
                actionTypeId = 2;
                using var checkCmd = new SqlCommand("spGetUserById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                checkCmd.Parameters.AddWithValue("@Id", userdto.Id);
                using var reader = await checkCmd.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return NotFound($"User with Id {userdto.Id} does not exist");
                await reader.CloseAsync();
            }
            else
            {
                actionTypeId = 1;
                using var emailCheckcmd = new SqlCommand("spCheckUserEmailExists", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                emailCheckcmd.Parameters.AddWithValue("@EmailId", userdto.Email);
                var exists = Convert.ToInt32(await emailCheckcmd.ExecuteScalarAsync());
                if (exists > 0)
                    return Conflict($"User with Email '{userdto.Email}' already exists.");
            }

            var passwordHash = PasswordHelper.HashPassword(userdto.Password);

            using var cmd = new SqlCommand("spCreateOrUpdateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", userdto.Id);
            cmd.Parameters.AddWithValue("@Name", userdto.Name);
            cmd.Parameters.AddWithValue("@EmailId", userdto.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@LocationId", userdto.LocationId);
            cmd.Parameters.AddWithValue("@IsActive", true);
            cmd.Parameters.AddWithValue("@ActionTypeId", actionTypeId);
            cmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);
            //await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            string message = userdto.Id == null ? "User created successfully." : $"User with Id{userdto.Id} updated successfully";
            return Ok(message);
        }


        [HttpPost("validate")]
        public async Task<IActionResult> ValidateUser([FromBody] Logindto logindto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("spGetUserByEmail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@EmailId", logindto.Email);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return Unauthorized("Invalid email or password");

            await reader.ReadAsync();
            var storedHash = reader["PasswordHash"].ToString();
            var emailFromDb = reader["EmailId"].ToString();

            bool isValid = PasswordHelper.VerifyPassword(logindto.Password, storedHash!);
            if (!isValid)
                return Unauthorized("Invalid email or password");


            var userObj = new
            {
                id = Convert.ToInt32(reader["Id"]),
                name = reader["Name"]?.ToString(),
                email = emailFromDb,
                locationId = Convert.ToInt32(reader["LocationId"]),
                isActive = Convert.ToBoolean(reader["IsActive"]),
                actionTypeId = Convert.ToInt32(reader["ActionTypeId"]),
                actionDate = Convert.ToDateTime(reader["ActionDate"]).ToString("yyyy-MM-dd")
            };

            var token = _tokenService.GenerateToken(emailFromDb!);

            return Ok(new
            {
                token = token,
                user = userObj,
                message = "Login successful. ",
                success = true
            });
        }


        //this is to get current user details based on the project name
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(emailClaim)) return Unauthorized();

            using var conn = GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand("spGetUserByEmail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@EmailId", emailClaim);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return NotFound();

            await reader.ReadAsync();
            var userObj = new
            {
                id = Convert.ToInt32(reader["Id"]),
                name = reader["Name"]?.ToString(),
                email = reader["EmailId"]?.ToString(),
                locationId = Convert.ToInt32(reader["LocationId"]),
                isActive = Convert.ToBoolean(reader["IsActive"]),
                actionTypeId = Convert.ToInt32(reader["ActionTypeId"]),
                actionDate = Convert.ToDateTime(reader["ActionDate"]).ToString("yyyy-MM-dd")
            };

            return Ok(userObj);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id == null)
            {
                return BadRequest("A valid user Id must be provided.");
            }
            using var conn = GetConnection();
            using var checkCmd = new SqlCommand("spGetUserById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            checkCmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                return NotFound($"User with ID {id} does not exist.");
            }
            await reader.CloseAsync();
            using var cmd = new SqlCommand("spDeleteUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
            return Ok($"User with ID {id} has been deleted successfully");
        }
    }
}