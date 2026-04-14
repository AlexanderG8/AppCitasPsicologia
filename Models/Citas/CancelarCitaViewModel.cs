using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Citas
{
    public class CancelarCitaViewModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string NombrePsicologo { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "El motivo de cancelación es requerido")]
        [StringLength(maximumLength: 1000)]
        [Display(Name = "Motivo de cancelación")]
        public string MotivoCancelacion { get; set; }
    }
}
