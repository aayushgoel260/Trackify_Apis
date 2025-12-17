using System.Text.Json.Serialization;

namespace TrackifyApis.Models
{
    public class UserLeaveDto
    {

        public int? Id { get; set; } 

        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public int LeaveTypeId { get; set; }
        public string? LeaveStatus { get; set; }
        //public int ActionTypeId { get; set; }

        [JsonIgnore]
        public DateTime ActionDate { get; set; }

    }
}
