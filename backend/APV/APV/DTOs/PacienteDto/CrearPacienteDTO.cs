using APV.Entities;
using System.ComponentModel.DataAnnotations;

namespace APV.DTOs.PacienteDto
{
    public class CrearPacienteDTO
    {
        [Required]
        public string nombre { get; set; }
        [Required]
        public string propietario { get; set; }
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        public DateTime fecha { get; set; } = DateTime.Now;
        [Required]
        public string sintomas { get; set; }

        // Clave foránea
        public int VeterinarioId { get; set; }

        // Propiedad de navegación
        public Veterinario Veterinario { get; set; }

    }
}
