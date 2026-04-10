using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Roles
{
    public class Opciones
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteCodigoOpcion", controller: "Opciones", AdditionalFields = nameof(Id))]
        public string NombreOpcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Controlador { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Accion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
