using System.ComponentModel.DataAnnotations;

namespace APV.DTOs.VeterinarioDto
{
    public class VeterinarioAutenticarDTO
    {      
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
