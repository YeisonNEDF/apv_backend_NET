using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace APV.Entities
{
    public class Veterinario
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40)]
        public string nombre { get; set; }
        public string password { get; set; }
        [Required]
        [EmailAddress]
        public string email { get; set; }        
        public string telefono { get; set; }
        public string web { get; set; }
        public string codigoUnico { get; set; }
        public bool cuentaConfirmada { get; set; } = false;        
        public string codigoAleatorio { get; set; }

        // Propiedad de navegación
        public ICollection<Paciente> Pacientes { get; set; }
        public Veterinario()
        {
            codigoAleatorio = GenerarCodigoAleatorio();
        }
        private string GenerarCodigoAleatorio()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8); // Ejemplo de código aleatorio
        }
    }
}
