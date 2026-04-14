using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCitasPsicologia.Models.Citas
{
    public class Citas
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PsicologoId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 1200)]
        [PrimeraLetraMayuscula]
        public string DetalleCita { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Asistencia { get; set; } = null;
        [StringLength(maximumLength: 1200)]
        [PrimeraLetraMayuscula]
        public string MotivoPostergacion { get; set; } = null;
        public string DocPago { get; set; } = null;
        public DateTime? FechaCancelacion { get; set; }
        public string MotivoCancelacion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; } = null;
        public DateTime? FechaEliminado { get; set; } = null;

        // Propiedades de display (solo lectura, no se mapean a columnas)
        [NotMapped] public string NombreCliente { get; set; }
        [NotMapped] public string NombrePsicologo { get; set; }
    }
}
