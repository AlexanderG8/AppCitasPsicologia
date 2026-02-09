using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models
{
    public class Roles
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteCodigoRol", controller: "Roles")]
        public string CodigoRol { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteNombreRol", controller:"Roles")]
        public string NombreRol { get; set; }
    }
}
