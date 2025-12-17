using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackifyApis.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TrackifyApis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLeavesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UserLeavesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLeave>>> GetUserLeaves()
        {
            var leaves = new List<UserLeave>();
            using var conn = GetConnection();
            using var cmd = new SqlCommand("sp_GetAllUserLeaves", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                leaves.Add(new UserLeave
                {

                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Date = DateOnly.FromDateTime(reader.GetDateTime(2)),
                    LeaveTypeId = reader.GetInt32(3),
                    ActionTypeId = reader.GetInt32(4),
                    ActionDate = DateOnly.FromDateTime(reader.GetDateTime(5))

                });
            }
            return Ok(leaves);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserLeavesByUserId(int userId, string? startDate = null, string? endDate = null)
        {
            var leaves = new List<object>();
            using var conn = GetConnection();

            // Build SQL query with date filtering if provided
            string sql = @"
                SELECT ul.Id, ul.UserId, ul.Date, ul.LeaveTypeId, ul.ActionTypeId, ul.ActionDate,
                       CASE ul.LeaveTypeId 
                           WHEN 0 THEN 'Present'
                           WHEN 1 THEN 'WFH' 
                           WHEN 2 THEN 'Full Leave'
                           WHEN 3 THEN 'Half Day'
                           WHEN 4 THEN 'Tentative'
                           ELSE 'Present'
                       END as LeaveStatus
                FROM UserLeave ul 
                WHERE ul.UserId = @UserId";

            if (!string.IsNullOrEmpty(startDate))
                sql += " AND ul.Date >= @StartDate";
            if (!string.IsNullOrEmpty(endDate))
                sql += " AND ul.Date <= @EndDate";

            sql += " ORDER BY ul.Date";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            if (!string.IsNullOrEmpty(startDate))
                cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(startDate));
            if (!string.IsNullOrEmpty(endDate))
                cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(endDate));

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Return in dictionary form as expected by frontend
            var leaveData = new Dictionary<string, string>();
            while (await reader.ReadAsync())
            {
                var date = reader.GetDateTime("Date").ToString("yyyy-MM-dd");
                var status = reader.GetString("LeaveStatus");
                leaveData[date] = status;
            }

            return Ok(leaveData);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateUserLeave([FromBody] UserLeaveDto leave)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var conn = GetConnection();
            await conn.OpenAsync();
            var statusMap = new Dictionary<string, int>
            {
                { "Present", 0 },
                { "WFH", 1 },
                { "Full Leave", 2 },
                { "Half Day", 3 },
                { "Tentative", 4 }
            };

            if (!string.IsNullOrEmpty(leave.LeaveStatus))
            {
                if (statusMap.TryGetValue(leave.LeaveStatus, out int mappedId))
                {
                    leave.LeaveTypeId = mappedId;
                }
            }

            var allowedLeaveIds = statusMap.Values.ToArray();
            if (!allowedLeaveIds.Contains(leave.LeaveTypeId))
                return BadRequest("Invalid LeaveTypeId");


            using var userCheckCmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[User] WHERE Id = @Id", conn);
            userCheckCmd.Parameters.AddWithValue("@Id", leave.UserId);
            var userExists = (int)await userCheckCmd.ExecuteScalarAsync() > 0;
            if (!userExists)
                return NotFound($"User with Id {leave.UserId} does not exist.");

            using var existingCmd = new SqlCommand(
                "SELECT Id FROM UserLeave WHERE UserId = @UserId AND Date = @Date", conn);
            existingCmd.Parameters.AddWithValue("@UserId", leave.UserId);
            existingCmd.Parameters.AddWithValue("@Date", leave.Date.Date);

            var existingId = await existingCmd.ExecuteScalarAsync();

            if (leave.LeaveTypeId == 0)
            {
                //delete the record if the status is present
                if (existingId != null)
                {
                    using var deleteCmd = new SqlCommand("DELETE FROM UserLeave WHERE Id = @Id", conn);
                    deleteCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(existingId));
                    await deleteCmd.ExecuteNonQueryAsync();
                    return Ok("Leave record removed as status is 'Present'.");
                }
                else
                {
                    return Ok("No leave record to remove; user is marked as 'Present'.");
                }
            }
            else
            {
                if (existingId != null)
                {
                    // Update existing record
                    leave.Id = Convert.ToInt32(existingId);
                    using var updateCmd = new SqlCommand(@"
                    UPDATE UserLeave 
                    SET LeaveTypeId = @LeaveTypeId, ActionTypeId = 2, ActionDate = @ActionDate
                    WHERE Id = @Id", conn);

                    updateCmd.Parameters.AddWithValue("@Id", leave.Id.Value);
                    updateCmd.Parameters.AddWithValue("@LeaveTypeId", leave.LeaveTypeId);
                    updateCmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);

                    await updateCmd.ExecuteNonQueryAsync();
                    return Ok($"UserLeave with ID {leave.Id} updated successfully.");
                }
                else
                {
                    // Create new record
                    using var insertCmd = new SqlCommand(@"
                    INSERT INTO UserLeave (UserId, Date, LeaveTypeId, ActionTypeId, ActionDate)
                
                    VALUES (@UserId, @Date, @LeaveTypeId, 1, @ActionDate);
                    SELECT SCOPE_IDENTITY();", conn);

                    insertCmd.Parameters.AddWithValue("@UserId", leave.UserId);
                    insertCmd.Parameters.AddWithValue("@Date", leave.Date.Date);
                    insertCmd.Parameters.AddWithValue("@LeaveTypeId", leave.LeaveTypeId);
                    insertCmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);

                    var newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
                    leave.Id = newId;
                    return Ok("New Leave for the user has been created");
                }
            }
        }





        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var checkCmd = new SqlCommand("spGetUserLeaveById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            checkCmd.Parameters.AddWithValue("@Id", id);
            UserLeave? leaveToDelete = null;
            using var reader = await checkCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())

            {
                leaveToDelete = new UserLeave
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Date = DateOnly.FromDateTime(reader.GetDateTime(2)),
                    LeaveTypeId = reader.GetInt32(3),
                    ActionTypeId = reader.GetInt32(4),
                    ActionDate = DateOnly.FromDateTime(reader.GetDateTime(5))
                };
            }
            else
            {
                return NotFound($"User Leave with ID {id} does not exist.");

            }
            await reader.CloseAsync();
            using var cmd = new SqlCommand("spDeleteUserLeave", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();


            using var logCmd = new SqlCommand(@"
        INSERT INTO UserLeaveLog (UserId, Date, LeaveTypeId, ActionTypeId, ActionDate, InsertDate)
        VALUES (@UserId, @Date, @LeaveTypeId, @ActionTypeId, @ActionDate, @InsertDate)", conn);

            logCmd.Parameters.AddWithValue("@UserId", leaveToDelete.UserId);
            logCmd.Parameters.AddWithValue("@Date", leaveToDelete.Date.ToDateTime(TimeOnly.MinValue));
            logCmd.Parameters.AddWithValue("@LeaveTypeId", leaveToDelete.LeaveTypeId);
            logCmd.Parameters.AddWithValue("@ActionTypeId", 3); // DELETE(3)
            logCmd.Parameters.AddWithValue("@ActionDate", DateTime.Today);
            logCmd.Parameters.AddWithValue("@InsertDate", DateTime.Now);

            await logCmd.ExecuteNonQueryAsync();

            return Ok($"User Leave with ID {id} has been deleted successfully");
        }
    }
}
