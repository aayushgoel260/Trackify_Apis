namespace TrackifyApis.Models
{
    public class EmployeeProjectInfoDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string EmailId { get; set; } = "";
        public int LocationId { get; set; }
        public string LocationName { get; set; } = "NA";
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
        public DateOnly ActionDate { get; set; }
    }
}
