using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppCitasPsicologia.Models.Empresas
{
    public class Empresas
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 11)]
        [Remote(action: "VerificarExisteRuc", controller: "Empresas", AdditionalFields = nameof(Id))]
        public string Ruc { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteNombreEmpresa", controller: "Empresas", AdditionalFields = nameof(Id))]
        public string NombreEmpresa { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Encargado { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 500)]
        [PrimeraLetraMayuscula]
        public string Direccion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
