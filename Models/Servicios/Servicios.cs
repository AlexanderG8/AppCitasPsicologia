using AppCitasPsicologia.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Servicios
{
    public class Servicios
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string NombreServicio { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal CostoServicio { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Descripcion { get; set; } = null;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
