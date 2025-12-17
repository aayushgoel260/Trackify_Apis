using System.ComponentModel.DataAnnotations;

namespace TrackifyApis.Models
{
    public class HolidayDTO
    {
        [Required]
        [StringLength(25)]
        public string Name { get; set; }
        [Required]

        public DateTime Date { get; set; }
        [Range(1,int.MaxValue)]
        public int LocationId { get; set; }
        public bool IsOptional { get; set; }
    }
}
