using APV.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace APV.Helper
{
    public class HashPassword
    {
        private readonly ApplicationDbContext context;

        public HashPassword(ApplicationDbContext context)
        {
            this.context = context;
        }

        //Creamos el usuario con password hasheada
        public async Task RegistrarUsuario(string _nombre, string _email, string _password)
        {
            Veterinario veterinario = new Veterinario();

            // Verificar si la contraseña ya está hasheada
            if(!IsHashed(_password))
            {
                veterinario.password = BCrypt.Net.BCrypt.HashPassword(_password);
            }

            veterinario.nombre = _nombre;
            veterinario.email = _email;               
            
            //Guardamos el veterinario con el password hash
            context.Veterinarios.Add(veterinario);
            await context.SaveChangesAsync();
        }


        public async Task CambiarPassword(int id, string password)
        {
            //Buscar al usuario por ID
            var veterinario = await context.Veterinarios.FindAsync(id);
            if (veterinario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            veterinario.codigoAleatorio = null;
            context.Entry(veterinario).Property(e => e.codigoAleatorio).IsModified = true;

            //Verificar si el password ya fue hasheada
            IsHashed(password);


            // Verificar si la contraseña ya está hasheada
            if (!IsHashed(password))
            {
                veterinario.password = BCrypt.Net.BCrypt.HashPassword(password);
            }
            // Guardar los cambios
            context.Entry(veterinario).Property(e => e.password).IsModified = true;
            await context.SaveChangesAsync();
        }

        public async Task ActualizarPassword(int id, string password)
        {
            //Buscar al usuario por ID
            var veterinario = await context.Veterinarios.FindAsync(id);
            if (veterinario == null)
            {
                throw new Exception("Usuario no encontrado");
            }
           
            //Verificar si el password ya fue hasheada
            IsHashed(password);


            // Verificar si la contraseña ya está hasheada
            if (!IsHashed(password))
            {
                veterinario.password = BCrypt.Net.BCrypt.HashPassword(password);
            }
            // Guardar los cambios
            context.Entry(veterinario).Property(e => e.password).IsModified = true;
            await context.SaveChangesAsync();
        }


        //Verificar si el password ya fue hasheada
        private bool IsHashed(string password)
        {
            //La expresión regular para los hashes bcrypt
            var passwordHashed = new Regex(@"^\$2[ayb]\$.{56}$");
            return passwordHashed.IsMatch(password);
        }


        public async Task<bool> VerificarPassword(string _email, string _password)
        {
            var veterinario = await context.Veterinarios.FirstOrDefaultAsync(u => u.email == _email);
            if (veterinario == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(_password, veterinario.password);
        }
    }
}
