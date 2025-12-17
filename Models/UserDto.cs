using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TrackifyApis.Models
{
    public class UserDto:IValidatableObject
    {
        public int? Id { get; set; }

        [Required(ErrorMessage ="Name is required")]
        //[StringLength(25, MinimumLength = 2, ErrorMessage = "Name can contain characters between range of 2 to 25")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage ="Email is required")]

        [EmailAddress(ErrorMessage ="Invalid email format")]
        [StringLength(50,ErrorMessage ="Email cannot exceed 50 characters")]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = "Password is required")]
        [StringLength(14, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 14 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])(?!.*\s).{6,14}$",
    ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and no spaces.")]
        public string Password { get; set; } = null!;


        [Required(ErrorMessage ="LocationId is required")]

        public int LocationId { get; set; }

        [JsonIgnore]
        public DateOnly ActionDate { get; set; }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Name: Only alphabets and single spaces
            var cleanedName = Regex.Replace(Name.Trim(), @"\s{2,}", " ");

            if (cleanedName.Length < 2 || cleanedName.Length > 25)
            {
                yield return new ValidationResult("Name must be between 2 and 25 characters.", new[] { nameof(Name) });
            }

            if (!Regex.IsMatch(cleanedName, @"^[A-Za-z]+(?: [A-Za-z]+)*$"))
            {
                yield return new ValidationResult("Name must contain only alphabets", new[] { nameof(Name) });
            }

            // Email: Must end with .com

            if (Email.Contains(" "))
            {
                yield return new ValidationResult("Email must not contain spaces.", new[] { nameof(Email) });
            }

            if (!Email.EndsWith("@innovasolutions.com", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Invalid Email ", new[] { nameof(Email) });
            }

            Name = cleanedName; 
        }


    }
}
