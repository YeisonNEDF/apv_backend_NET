using APV.DTOs.PacienteDto;
using APV.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APV.Controllers.Pacientes
{
    [ApiController]
    [Route("api/pacientes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PacienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public PacienteController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<PacienteDTO>>> Get()
        {
            var userIdClaim = HttpContext.User.Claims.Where(claim => claim.Type == "UserId").FirstOrDefault();
            if (userIdClaim == null)
            {
                return NotFound(new { Error = $"No se encontró el Veterinario" });
            }

            //Buscamos los pacientes y los filtramos por el veterinario asociado 
            var pacientes = await context.Pacientes.Where(p => p.VeterinarioId == int.Parse(userIdClaim.Value)).ToListAsync();
            var dtos = mapper.Map<List<PacienteDTO>>(pacientes);

            return Ok(dtos);
        }

        [HttpGet("obtenerPaciente/{id:int}", Name = "obtenerPaciente")]
        public async Task<ActionResult<PacienteDTO>> GetById(int id)
        {
            var entidad = await context.Pacientes.FirstOrDefaultAsync(x => x.Id == id);            

            if (entidad != null)
            {
                var userIdClaim = HttpContext.User.Claims.Where(claim => claim.Type == "UserId").FirstOrDefault();
                //Validamos que sea el veterinario que creo el paciente
                if (entidad.VeterinarioId != int.Parse(userIdClaim.Value))
                {
                    return BadRequest(new { Error = "Accioón no válidad" });
                }
            }
            else
            {
                return BadRequest(new { Error = "Paciente no existe" });
            }           

            var dto = mapper.Map<PacienteDTO>(entidad);

            return dto;
        }

        [HttpPost("agregarPaciente")]        
        public async Task<ActionResult> PostPaciente([FromBody] CrearPacienteDTO pacienteDTO)
        {
            /*var userIdClaim = HttpContext.User.Claims.Where(claim => claim.Type == "UserId").FirstOrDefault();
             * var userIdClaim = User.FindFirst("UserId");
            */

            // Verificar si el Veterinario existe mediante UserId(Claim) que vienen en el Token 
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return NotFound(new { Error = $"No se encontró el Veterinario" });
            }

            var paciente = mapper.Map<Paciente>(pacienteDTO);
            paciente.VeterinarioId = int.Parse(userIdClaim.Value);
            context.Add(paciente);
            await context.SaveChangesAsync();
            //retornamos el pacienteDTO creado
            var pacientC = mapper.Map<PacienteDTO>(paciente);

            return new CreatedAtRouteResult("obtenerPaciente", new { id = pacientC.Id }, pacientC);
        }

        [HttpPut("actualizarPaciente/{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] CrearPacienteDTO pacienteDTO)
        {
            var entidad = await context.Pacientes.FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
            {
                return BadRequest(new { Error = "Paciente no existe" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "UserId");
            if (userIdClaim == null || entidad.VeterinarioId != int.Parse(userIdClaim.Value))
            {
                return BadRequest(new { Error = "Acción no válida" });
            }

            // Actualizar solo los campos necesarios
            entidad.nombre = pacienteDTO.nombre;
            entidad.propietario = pacienteDTO.propietario;
            entidad.email = pacienteDTO.email;
            entidad.fecha = pacienteDTO.fecha;
            entidad.sintomas = pacienteDTO.sintomas;

            context.Entry(entidad).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return Ok(entidad);
        }


        [HttpDelete("eliminarPaciente/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entidad = await context.Pacientes.FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
            {
                return BadRequest(new { Error = "Paciente no existe" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "UserId");

            if (userIdClaim == null || entidad.VeterinarioId != int.Parse(userIdClaim.Value))
            {
                return BadRequest(new { Error = "Acción no válida" });
            }

            context.Pacientes.Remove(entidad);
            await context.SaveChangesAsync();

            return Ok(new { Message = "Paciente eliminado correctamente" });
        }

    }
}
