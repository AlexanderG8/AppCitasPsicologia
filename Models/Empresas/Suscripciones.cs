using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Empresas
{
    public class Suscripciones
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteNombreSuscripcion", controller: "Suscripciones", AdditionalFields = nameof(Id))]
        public string NombreSuscripcion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int CantMeses { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
