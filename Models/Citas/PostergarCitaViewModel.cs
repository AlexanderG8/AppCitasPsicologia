using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Citas
{
    public class PostergarCitaViewModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string NombrePsicologo { get; set; }
        public DateTime FechaReservaActual { get; set; }
        public TimeSpan HoraInicioActual { get; set; }
        public TimeSpan HoraFinActual { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 1200)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Motivo de postergación")]
        public string MotivoPostergacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nueva fecha de reserva")]
        public DateTime NuevaFechaReserva { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nueva hora de inicio")]
        public TimeSpan NuevaHoraInicio { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nueva hora de fin")]
        public TimeSpan NuevaHoraFin { get; set; }
    }
}
