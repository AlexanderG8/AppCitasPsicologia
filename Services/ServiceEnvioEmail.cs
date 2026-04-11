using System.Net;
using System.Net.Mail;

namespace NETPortafolio.Services
{
    public interface IServiceEmail
    {
        Task EnviarCorreoIngresarContrasena(string emailDestinatario, string nombre, string link);
    }

    public class ServiceEmailGmail : IServiceEmail
    {
        private readonly IConfiguration configuration;

        public ServiceEmailGmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarCorreoIngresarContrasena(string emailDestinatario, string nombre, string link)
        {
            var emailEmisor = configuration.GetValue<string>("CONFIGURATION_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURATION_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURATION_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURATION_EMAIL:PUERTO");

            var cuerpo = $@"
                <p>Hola <strong>{nombre}</strong>,</p>
                <p>Tu cuenta en <strong>PsicoCitas</strong> ha sido creada. Para establecer tu contraseña, haz clic en el siguiente enlace:</p>
                <p><a href=""{link}"" style=""background:#4f46e5;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;"">Establecer contraseña</a></p>
                <p>Este enlace es válido por <strong>24 horas</strong>.</p>
                <p>Si no solicitaste este acceso, ignora este mensaje.</p>";

            var smtpClient = new SmtpClient(host, puerto)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailEmisor, password)
            };

            var mensaje = new MailMessage(emailEmisor, emailDestinatario,
                "Bienvenido a PsicoCitas — Establece tu contraseña", cuerpo)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mensaje);
        }
    }
}
