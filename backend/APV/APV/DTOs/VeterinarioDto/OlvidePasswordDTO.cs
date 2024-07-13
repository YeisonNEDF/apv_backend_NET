using System.ComponentModel.DataAnnotations;

namespace APV.DTOs.VeterinarioDto
{
    public class OlvidePasswordDTO
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
    }
}
