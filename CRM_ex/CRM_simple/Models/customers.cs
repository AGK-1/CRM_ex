using System.ComponentModel.DataAnnotations;

public class Customers // Class names should usually start with an uppercase letter
{
    public int Id { get; set; } // Use public access modifier for EF to recognize it as a property

    [Required(ErrorMessage = "Company Name is required.")]
    public string CompanyName { get; set; } // Use PascalCase for property names

    [Required(ErrorMessage = "Contact Name is required.")]
    public string ContactName { get; set; } // Use PascalCase for property names

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{2}-\d{2}$", ErrorMessage = "Phone number must be in the format 050-750-12-99.")]
    public string Phone { get; set; } // Use PascalCase for property names
}
