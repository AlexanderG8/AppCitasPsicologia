using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Citas
{
    public class DetallesCitas
    {
        public int Id { get; set; }
        public int CitaId { get; set; }

        // Evaluación de la sesión
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 500)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Motivo de consulta")]
        public string MotivoConsulta { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Estado emocional")]
        public string EstadoEmocional { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de ansiedad debe estar entre 1 y 10")]
        [Display(Name = "Nivel de ansiedad")]
        public int? NivelAnsiedad { get; set; }

        [Range(1, 10, ErrorMessage = "El nivel de depresión debe estar entre 1 y 10")]
        [Display(Name = "Nivel de depresión")]
        public int? NivelDepresion { get; set; }

        [StringLength(maximumLength: 500)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Síntomas")]
        public string Sintomas { get; set; } = null;

        // Desarrollo de la sesión
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 2000)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Observaciones de la sesión")]
        public string ObservacionesSesion { get; set; }

        [StringLength(maximumLength: 500)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Temas abordados")]
        public string TemasAbordados { get; set; } = null;

        [StringLength(maximumLength: 300)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Técnica aplicada")]
        public string TecnicaAplicada { get; set; } = null;

        [StringLength(maximumLength: 1000)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Respuesta del paciente")]
        public string RespuestaPaciente { get; set; } = null;

        // Plan y seguimiento
        [StringLength(maximumLength: 300)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; } = null;

        [StringLength(maximumLength: 1000)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Plan terapéutico")]
        public string PlanTerapeutico { get; set; } = null;

        [StringLength(maximumLength: 1000)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Tareas asignadas")]
        public string TareasAsignadas { get; set; } = null;

        [StringLength(maximumLength: 500)]
        [PrimeraLetraMayuscula]
        [Display(Name = "Próximo objetivo")]
        public string ProximoObjetivo { get; set; } = null;

        // Progreso
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nivel de progreso")]
        public string NivelProgreso { get; set; }

        // Control
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; } = null;
    }
}
