using System.ComponentModel.DataAnnotations;

namespace APV.DTOs.VeterinarioDto
{
    public class VeterinarioDTO
    {
 
        [Required]
        public string nombre { get; set; }
        public string password { get; set; }

        [Required]
        public string email { get; set; }

        public string telefono { get; set; }

        public string web { get; set; }

        public bool cuentaConfirmada { get; set; } = false;

        public string codigoAleatorio { get; set; }
    }
}
