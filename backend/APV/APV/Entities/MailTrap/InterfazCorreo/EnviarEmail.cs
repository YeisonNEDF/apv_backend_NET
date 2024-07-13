using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace APV.Entities.MailTrap.InterfazCorreo
{
    public class EnviarEmail : IEnvioCorreo
    {
        private readonly EnvioCorreo _envioCorreo;

        public EnviarEmail(IOptions<EnvioCorreo> envioCorreo)
        {
            _envioCorreo = envioCorreo.Value;
        }

        public async Task EmailRegistroAsync(string to, string name, string codigoA)
        {
            using (var client = new SmtpClient(_envioCorreo.Host, _envioCorreo.Port))
            {
                client.Credentials = new NetworkCredential(_envioCorreo.Username, _envioCorreo.Password);
                client.EnableSsl = _envioCorreo.EnableSsl;

                //Cuerpo del correo
                var body = new StringBuilder();
                body.AppendLine("APV - Administrador de Pacientes de Veterinaria.</span>");
                body.AppendLine("Comprueba tu cuenta en APV,");
                body.AppendLine($"Hola: {name},");
                body.AppendLine($"Tu cuenta ya esta lista, solo debes comprobarla en el siguiente enlace: " +
                    $"<a href=\"http://localhost:3000/confirmar/{codigoA}\">Comprobar Cuenta</a>");
                body.AppendLine("<p>Si tu no creaste esta cuenta, puedes ignorar este mensaje.</p>");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_envioCorreo.From),
                    Subject = $"Hola, {name}",
                    Body = body.ToString(),
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task EmailOlvidePasswordAsync(string to, string name, string codigoA)
        {
            using (var client = new SmtpClient(_envioCorreo.Host, _envioCorreo.Port))
            {
                client.Credentials = new NetworkCredential(_envioCorreo.Username, _envioCorreo.Password);
                client.EnableSsl = _envioCorreo.EnableSsl;

                //Cuerpo del correo
                var body = new StringBuilder();
                body.AppendLine("APV - Administrador de Pacientes de Veterinaria.</span>");
                body.AppendLine("Reestablece tu Password");
                body.AppendLine($"Hola: {name}, has solicitado reestablecer tu password.");
                body.AppendLine($"Sigue el siguiente enlace para generar un nuevo password: " +
                    $"<a href=\"http://localhost:3000/olvide-password/{codigoA}\">Reestablecer Password/a>");

                body.AppendLine("<p>Si tu no creaste esta cuenta, puedes ignorar este mensaje.</p>");


                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_envioCorreo.From),
                    Subject = $"Restablece tu Password",
                    Body = body.ToString(),
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}