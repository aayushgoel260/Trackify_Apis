using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrackifyApis.Models
{
    public class UserProjectDto
    {
        public int? Id { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoleId { get; set; }
        [Required]
        public bool IsActive { get; set; }

        //public int ActionTypeId { get; set; }

        [JsonIgnore]
        public DateOnly ActionDate { get; set; }
    }
}
