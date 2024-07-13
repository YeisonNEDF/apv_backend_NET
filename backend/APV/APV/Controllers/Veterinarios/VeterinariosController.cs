using APV.DTOs.VeterinarioDto;
using APV.Entities;
using APV.Entities.MailTrap.InterfazCorreo;
using APV.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APV.Controllers.Veterinarios
{
    [ApiController]
    [Route("api/veterinarios")]
    public class VeterinariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly HashPassword hashPassword;
        private readonly GenerarToken generarToken;
        private readonly IEnvioCorreo envioCorreo;

        public VeterinariosController(ApplicationDbContext context, IMapper mapper, HashPassword hashPassword,
            GenerarToken generarToken, IEnvioCorreo envioCorreo)
        {
            this.context = context;
            this.mapper = mapper;
            this.hashPassword = hashPassword;
            this.generarToken = generarToken;
            this.envioCorreo = envioCorreo;
        }

        [HttpGet]
        public async Task<ActionResult<List<VeterinarioDTO>>> Get()
        {
            var entidades = await context.Veterinarios.ToListAsync();
            var dtos = mapper.Map<List<VeterinarioDTO>>(entidades);
            return dtos;
        }


        [HttpGet("{id:int}", Name = "obtenerPerfilId")]
        public async Task<ActionResult<VeterinarioDTO>> GetById(int id)
        {
            var entidades = await context.Veterinarios.FirstOrDefaultAsync(x => x.Id == id);

            if (entidades == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<VeterinarioDTO>(entidades);

            return Ok(dto);

        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Post([FromBody] VeterinarioDTO veterinarioDTO)
        {
            //Validar si el correo ya fue registrado
            var correoExiste = await context.Veterinarios.FirstOrDefaultAsync(x => x.email == veterinarioDTO.email);

            if (correoExiste != null)
            {
                return BadRequest(new { Error = "El usuario ya esta registrado" });
            }

            try
            {
                var entidad = mapper.Map<Veterinario>(veterinarioDTO);

                //Hash Password
                await hashPassword.RegistrarUsuario(veterinarioDTO.nombre, veterinarioDTO.email, veterinarioDTO.password);

                var veterinarioCreado = await context.Veterinarios.FirstOrDefaultAsync(x => x.email == veterinarioDTO.email);
                var veterinarioDTOCreado = mapper.Map<VeterinarioDTO>(veterinarioCreado);

                //Obtenemos l veterinario creado
                var veterinarioCreadoList = new
                    {
                     email = veterinarioCreado.email,
                     nombre = veterinarioCreado.nombre,
                     codigoA = veterinarioCreado.codigoAleatorio
                };

                //Enviamos el correo
                await envioCorreo.EmailRegistroAsync(veterinarioCreado.email, veterinarioCreado.nombre, veterinarioCreado.codigoAleatorio);
                

                //Retornamos el usuario creado
                return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, veterinarioCreado);

            }
            catch (Exception)
            {
                return BadRequest(new { Error = "Valide los datos obligatorios" });
            }

        }


        [HttpGet("confirmar/{codigo}")]
        public async Task<ActionResult<VeterinarioDTO>> GetConfirmar(string codigo)
        {
            var entidades = await context.Veterinarios.FirstOrDefaultAsync(x => x.codigoAleatorio == codigo);

            if (entidades == null)
            {
                return BadRequest(new { Error = "Codigo no valido" });
            }
            try
            {
                entidades.codigoAleatorio = null;
                entidades.cuentaConfirmada = true;

                context.Entry(entidades).Property(e => e.codigoAleatorio).IsModified = true;
                context.Entry(entidades).Property(e => e.cuentaConfirmada).IsModified = true;

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest(new { Error = "Se presentó un error al intentar confirmar la cuenta" });
            }

            return Ok(new { Message = "Usuario Confirmado correctamente" });

        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenAutenticacion>> PostLogin([FromBody] VeterinarioAutenticarDTO autenticarDTO)
        {
            //Buscamos el usuario
            var correoExiste = await context.Veterinarios.FirstOrDefaultAsync(x => x.email == autenticarDTO.email);
            //Validar si el correo exite
            if (correoExiste != null)
            {
                //Validar si la cuenta esta confirmada
                if (correoExiste.cuentaConfirmada == true)
                {
                    //Validar si el password es correcto
                    if (BCrypt.Net.BCrypt.Verify(autenticarDTO.password, correoExiste.password))
                    {
                        //Autenticar usuario
                        return Ok(new 
                        {   id = correoExiste.Id,
                            nombre = correoExiste.nombre,
                            email = correoExiste.email,
                            token = generarToken.ConstruirToken(autenticarDTO, correoExiste.Id)
                    });
                    }
                    else
                    {
                        return BadRequest(new { Error = "El Password Incorrecto." });
                    }
                }
                else
                {
                    return BadRequest(new { Error = "Tu cuenta no ha sido confirmada." });
                }
            }
            else
            {
                return BadRequest(new { Error = "El usuario no existe." });
            }
        }

        [HttpPost("olvide-password")]
        public async Task<ActionResult> OlvidePassword([FromBody] OlvidePasswordDTO olvidePassword)
        {
            //Buscamos el usuario
            var correoExiste = await context.Veterinarios.FirstOrDefaultAsync(x => x.email == olvidePassword.email);

            if (correoExiste != null)
            {
                try
                {
                    //Modificamos el campo codigoAleatorio y lo guardamos
                    correoExiste.codigoAleatorio = GenerarCodigoAleatorio();
                    context.Entry(correoExiste).Property(e => e.codigoAleatorio).IsModified = true;
                    await context.SaveChangesAsync();

                    //Enviamos el correo
                    await envioCorreo.EmailOlvidePasswordAsync(correoExiste.email, correoExiste.nombre, correoExiste.codigoAleatorio);                    

                    return Ok(new { Message = "Hemos enviado un email con las instrucciones" });
                }
                catch (Exception)
                {

                    return BadRequest(new { Error = "El Usuario no existe" });
                }
            }
            else
            {
                return BadRequest(new { Error = "El Usuario no existe" });
            }
        }

        [HttpGet("olvide-password/{codigo}")]
        public async Task<ActionResult> ValidarCodigoAleatorio(string codigo)
        {
            var codigoExiste =await context.Veterinarios.FirstOrDefaultAsync(x => x.codigoAleatorio == codigo);
            if (codigoExiste != null)
            {
                //El codigo existe el usuario el válido
                var response = new
                {
                    Mensaje = "Código válido y el usuario existe",
                    Codigo = codigoExiste.codigoAleatorio
                };
                return Ok(new { Message = response });
            }
            else
            {
                return BadRequest(new { Error = "Código no válido" });
            }
        }

        [HttpPost("olvide-password/{codigo}")]
        public async Task<ActionResult<NuevoPasswordDTO>> NuevoPassword(string codigo,[FromBody] NuevoPasswordDTO nuevoPasswordDTO)
        {
            var codigoExiste = await context.Veterinarios.FirstOrDefaultAsync(x => x.codigoAleatorio == codigo);
            if (codigoExiste != null)
            {
                try
                {
                    //Hash Password
                    await hashPassword.CambiarPassword(codigoExiste.Id, nuevoPasswordDTO.password);

                    return Ok(new { Message = "Password Modificado correctamente." });
                }
                catch (Exception)
                {

                    return BadRequest(new { Error = "Hubo un error al intenar modificar el password." });
                }
                
            }
            else
            {
                return BadRequest(new { Error = "Código no valido." });
            }
        }

        [HttpPut("actualizarPerfil/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Actualizar(int id, [FromBody] VeterinarioDTO veterinarioDTO)
        {
            //Validamos que este authenticado
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || id != int.Parse(userIdClaim.Value))
            {
                return Unauthorized(new { Error = "No esta autorizado" });                
            }
            //Buecamos que exista en BD
            var entidad = await context.Veterinarios.FirstOrDefaultAsync(x => x.Id == id);
            if (entidad == null)
            {
                return BadRequest("Hubo un error");
            }

            // Verificamos si el email ya existe en otro usuario
            var otroVeterinarioConMismoEmail = await context.Veterinarios
                .FirstOrDefaultAsync(x => x.email == veterinarioDTO.email && x.Id != id);
            if (otroVeterinarioConMismoEmail != null)
            {
                return BadRequest("El email ya está en uso por otro veterinario");
            }


            try
            {
                entidad.nombre = veterinarioDTO.nombre;
                entidad.email = veterinarioDTO.email;
                entidad.web = veterinarioDTO.web;
                entidad.telefono = veterinarioDTO.telefono;

                context.Entry(entidad).State = EntityState.Modified;
                await context.SaveChangesAsync();

                //Retornamos el usuario creado
                return Ok(entidad);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }            
        }

        [HttpPut("actualizarPassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> ActualizarPassword([FromBody] ActualizarPasswordDTO actualizarPasswordDTO)
        {
            // Validamos que esté autenticado
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { Error = "No está autorizado" });
            }

            var userId = int.Parse(userIdClaim.Value);

            // Buscamos que exista en BD
            var entidad = await context.Veterinarios.FirstOrDefaultAsync(x => x.Id == userId);
            if (entidad == null)
            {
                return BadRequest(new { Error = "Hubo un error" });
            }

            // Verificamos el password actual
            if (!BCrypt.Net.BCrypt.Verify(actualizarPasswordDTO.passwordActual, entidad.password))
            {
                return BadRequest(new { Error =  "El password actual es incorrecto" });
            }

            // Hasheamos el nuevo password
            entidad.password = BCrypt.Net.BCrypt.HashPassword(actualizarPasswordDTO.nuevoPassword);

            // Guardamos los cambios en la base de datos
            context.Entry(entidad).Property(e => e.password).IsModified = true;
            await context.SaveChangesAsync();

            return Ok(new { Message = "Password modificado correctamente." });
        }

        //Area provada
        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[AllowAnonymous] 
        public async Task<ActionResult<VeterinarioDTO>> GetPerfil()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { Error = "No esta autorizado" });
            }

            var userId = userIdClaim.Value;
            var entidades = await context.Veterinarios.FindAsync(int.Parse(userId));
            if (entidades == null)
            {
                return NotFound(new { Error = "Usuario no encontrado." });
            }

            
            var veterinarioObtenido = await context.Veterinarios.FirstOrDefaultAsync(x => x.email == entidades.email);
            var perfil = mapper.Map<VeterinarioDTO>(entidades);

            //Retornamos el usuario creado
            return CreatedAtAction(nameof(GetById), new { id = entidades.Id }, veterinarioObtenido);

        }

        private string GenerarCodigoAleatorio()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8); // Ejemplo de código aleatorio
        }
    }
}
