using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TrackifyApis.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserProjectsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UserProjectsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        [HttpGet]

        public async Task<ActionResult<IEnumerable<UserProject>>> GetUserProjects()
        {
            var userProjects = new List<UserProject>();
            using var conn = GetConnection();
            using var cmd = new SqlCommand("spGetUserProjects", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                userProjects.Add(new UserProject
                {


                    Id = reader.GetInt32(0),
                    ProjectId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    RoleId = reader.GetInt32(3),
                    IsActive = reader.GetBoolean(4),
                    ActionTypeId = reader.GetInt32(5),
                    ActionDate = DateOnly.FromDateTime(reader.GetDateTime(6))


                });
            }
            return Ok(userProjects);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateUserProject([FromBody] UserProjectDto userProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var conn = GetConnection();
            await conn.OpenAsync();



            using var userCheckCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[User] WHERE Id = @Id", conn);
            userCheckCmd.Parameters.AddWithValue("@Id", userProject.UserId);
            var userExists = (int)await userCheckCmd.ExecuteScalarAsync() > 0;
            if (!userExists)
                return BadRequest(new { success = false, message = $"User with Id {userProject.UserId} does not exist." });


            // Validate project exists
            using var projectCheckCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Project] WHERE Id = @Id", conn);
            projectCheckCmd.Parameters.AddWithValue("@Id", userProject.ProjectId);
            var projectExists = (int)await projectCheckCmd.ExecuteScalarAsync() > 0;
            if (!projectExists)
                return BadRequest("Invalid ProjectId");

            // Validate role exists
            using var roleCheckCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Role] WHERE Id = @Id", conn);
            roleCheckCmd.Parameters.AddWithValue("@Id", userProject.RoleId);
            var roleExists = (int)await roleCheckCmd.ExecuteScalarAsync() > 0;
            if (!roleExists)
                return BadRequest("Invalid RoleId");

            // Prevent duplicate assignment of same user to same project
            using var duplicateCheckCmd = new SqlCommand(@"
        SELECT COUNT(*) FROM [dbo].[UserProject]
        WHERE UserId = @UserId AND ProjectId = @ProjectId AND IsActive = 1", conn);
            duplicateCheckCmd.Parameters.AddWithValue("@UserId", userProject.UserId);
            duplicateCheckCmd.Parameters.AddWithValue("@ProjectId", userProject.ProjectId);

            var alreadyAssigned = (int)await duplicateCheckCmd.ExecuteScalarAsync() > 0;

            if (alreadyAssigned && !userProject.Id.HasValue)
            {
                // Get project name for better error message
                using var projectNameCmd = new SqlCommand("SELECT Name FROM [dbo].[Project] WHERE Id = @Id", conn);
                projectNameCmd.Parameters.AddWithValue("@Id", userProject.ProjectId);
                var projectName = await projectNameCmd.ExecuteScalarAsync() as string ?? "this project";

                return Conflict(new { success = false, message = $"User is already assigned to {projectName}." });
            }

            int actionTypeId;
            if (!userProject.Id.HasValue)
            {
                actionTypeId = 1; // Insert
            }
            else
            {
                actionTypeId = 2; // Update
                using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[UserProject] WHERE Id=@Id", conn);
                checkCmd.Parameters.AddWithValue("@Id", userProject.Id.Value);
                var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;
                if (!exists)
                {
                    return NotFound($"User project with Id {userProject.Id} not found.");
                }
            }

            try
            {
                // Call stored procedure
                using var cmd = new SqlCommand("spCreateOrUpdateUserProject", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Id", (object?)userProject.Id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProjectId", userProject.ProjectId);
                cmd.Parameters.AddWithValue("@UserId", userProject.UserId);
                cmd.Parameters.AddWithValue("@RoleId", userProject.RoleId);
                cmd.Parameters.AddWithValue("@IsActive", userProject.IsActive);
                cmd.Parameters.AddWithValue("@ActionTypeId", actionTypeId);
                cmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);

                await cmd.ExecuteNonQueryAsync();

                string message = userProject.Id == null
                    ? "User assigned to project successfully."
                    : "User assignment updated successfully.";

                return Ok(new { success = true, message });
            }
            catch (SqlException ex)
            {
                // Log the actual SQL error for debugging
                return BadRequest(new { success = false, message = "Database error occurred while assigning user to project.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "An error occurred while processing the request.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserProject(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using var fetchCmd = new SqlCommand("SELECT * FROM [dbo].[UserProject] WHERE Id = @Id", conn);
            fetchCmd.Parameters.AddWithValue("@Id", id);

            UserProject? projectToDelete = null;
            using var reader = await fetchCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                projectToDelete = new UserProject
                {
                    Id = reader.GetInt32(0),
                    ProjectId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    RoleId = reader.GetInt32(3),
                    IsActive = reader.GetBoolean(4),
                    ActionTypeId = reader.GetInt32(5),
                    ActionDate = DateOnly.FromDateTime(reader.GetDateTime(6))
                };
            }
            else
            {
                return NotFound($"UserProject with ID {id} does not exist.");
            }
            await reader.CloseAsync();

            using var deleteCmd = new SqlCommand("spDeleteUserProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            deleteCmd.Parameters.AddWithValue("@Id", id);
            await deleteCmd.ExecuteNonQueryAsync();

            using var logCmd = new SqlCommand(@"
        INSERT INTO UserProjectLog (ProjectId, UserId, RoleId, IsActive, ActionTypeId, ActionDate, InsertDate)
        VALUES (@ProjectId, @UserId, @RoleId, @IsActive, @ActionTypeId, @ActionDate, @InsertDate)", conn);

            logCmd.Parameters.AddWithValue("@ProjectId", projectToDelete.ProjectId);
            logCmd.Parameters.AddWithValue("@UserId", projectToDelete.UserId);
            logCmd.Parameters.AddWithValue("@RoleId", projectToDelete.RoleId);
            logCmd.Parameters.AddWithValue("@IsActive", false);
            logCmd.Parameters.AddWithValue("@ActionTypeId", 3);
            logCmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);
            logCmd.Parameters.AddWithValue("@InsertDate", DateTime.Now);

            await logCmd.ExecuteNonQueryAsync();

            return Ok($"UserProject with ID {id} has been deleted and logged successfully.");
        }
        [HttpGet("project/{projectName}/employees")]
        public async Task<IActionResult> GetEmployeesByProjectName(string projectName)
        {
            var employees = new List<EmployeeProjectInfoDto>();

            using var conn = GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
        SELECT u.Id, u.Name, u.EmailId, u.LocationId,l.Name AS LocationName, up.ProjectId, up.RoleId, up.ActionDate
        FROM [dbo].[UserProject] up
        INNER JOIN [dbo].[User] u ON up.UserId = u.Id
        INNER JOIN [dbo].[Project] p ON up.ProjectId = p.Id
LEFT JOIN [dbo].[Location] l ON u.LocationId = l.Id
        WHERE p.Name = @ProjectName AND up.IsActive = 1", conn);

            cmd.Parameters.AddWithValue("@ProjectName", projectName);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                employees.Add(new EmployeeProjectInfoDto
                {
                    UserId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    EmailId = reader.GetString(2),
                    LocationId = reader.GetInt32(3),
                    LocationName = reader.GetString(4),
                    ProjectId = reader.GetInt32(5),
                    RoleId = reader.GetInt32(6),
                    ActionDate = DateOnly.FromDateTime(reader.GetDateTime(7))
                });
            }

            return Ok(employees);
        }

    }
}