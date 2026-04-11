using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCitasPsicologia.Models.Empresas
{
    public class DetalleSuscripciones
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int EmpresaId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int SuscripcionId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime FechaInicio { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime FechaFin { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string DocPago { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
        [NotMapped]
        public IFileHttpResult DocPagoFile { get; set; }
    }
}
