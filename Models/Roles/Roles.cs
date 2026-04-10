using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Roles
{
    public class Roles
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteCodigoRol", controller: "Roles", AdditionalFields = nameof(Id))]
        public string CodigoRol { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteNombreRol", controller:"Roles", AdditionalFields = nameof(Id))]
        public string NombreRol { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; } = null;
        public DateTime? FechaEliminado { get; set; } = null;
    }
}
