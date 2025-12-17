namespace TrackifyApis.Models
{
    public class UpdateLeaveStatusDto
    {
        public int UserId { get; set; }
        public string Date { get; set; }
        public string LeaveStatus { get; set; }
    }
}
