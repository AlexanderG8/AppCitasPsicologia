using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Empresas
{
    public class Suscripciones
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NombreSuscripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int CantMeses { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
