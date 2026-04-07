using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Citas
{
    public class Citas
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PsicologoId { get; set; }
        public int ServicioId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 1200)]
        [PrimeraLetraMayuscula]
        public string DetalleCita { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Asistencia { get; set; } = null;
        public DateTime? FechaPostergacion { get; set; } = null;
        [StringLength(maximumLength: 1200)]
        [PrimeraLetraMayuscula]
        public string MotivoPostergacion { get; set; } = null;
        public DateTime? FechaIncioCita { get; set; } = null;
        public DateTime? FechaFinCita { get; set; } = null;
        public string DocPago { get; set; } = null;
        public string DocAdjuntos { get; set; } = null;
    } 
}
